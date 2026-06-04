using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using core.Models.Generic;
using core.Interfaces;
using core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;
using Shared.Helpers;

namespace core.UnitTests;

public class UploadingFileStateTests
{
    [Fact]
    public async Task WritePartFromPipeAsync_CopiesSegmentedDataToWriterAndReturnsSuccess()
    {
        var segments = new[]
        {
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6, 7, 8 }
        };

        var capturedWrites = new System.Collections.Generic.List<(byte[] Data, long Offset)>();
        var writer = new TestPhysicalFileWriter((buffer, offset) =>
        {
            capturedWrites.Add((buffer.ToArray(), offset));
        });
        var factory = new TestPhysicalFileWriterFactory(writer);

        var state = new UploadingFileState(CreateFileDto(totalFileParts: 1, partSize: 1024, fileSize: 8),
                                          "unused",
                                          Guid.NewGuid(),
                                          factory);

        var sequence = CreateSequence(segments);
        var reader = PipeReader.Create(sequence);

        var result = await state.WritePartFromPipeAsync(0, reader, CancellationToken.None, NullLogger<UploadingFileState>.Instance);

        var success = Assert.IsType<Success<UploadPartSuccess>>(result);
        Assert.Equal(8, success.Value.BytesWritten);
        Assert.Single(capturedWrites);
        Assert.Equal(0L, capturedWrites[0].Offset);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, capturedWrites[0].Data);
    }

    [Fact]
    public async Task WritePartFromPipeAsync_DuplicatePartDoNotIncrementPartsWritten()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        var writer = new TestPhysicalFileWriter();
        var factory = new TestPhysicalFileWriterFactory(writer);

        var state = new UploadingFileState(CreateFileDto(totalFileParts: 2, partSize: 4, fileSize: 8),
                                          "unused",
                                          Guid.NewGuid(),
                                          factory);

        var firstReader = PipeReader.Create(CreateSequence(payload));
        var firstResult = await state.WritePartFromPipeAsync(1, firstReader, CancellationToken.None, NullLogger<UploadingFileState>.Instance);

        Assert.IsType<Success<UploadPartSuccess>>(firstResult);
        Assert.Equal(1, state.PartsWritten);

        var secondReader = PipeReader.Create(CreateSequence(payload));
        var secondResult = await state.WritePartFromPipeAsync(1, secondReader, CancellationToken.None, NullLogger<UploadingFileState>.Instance);

        Assert.IsType<Success<UploadPartSuccess>>(secondResult);
        Assert.Equal(1, state.PartsWritten);
        Assert.Equal(2u, state.PartsBitfield);
    }

    [Fact]
    public async Task WritePartFromPipeAsync_TriggersCloseEventOnlyWhenAllPartsWrittenAndFlushesOnFinalPart()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        var flushCount = 0;
        var closeEventCount = 0;

        var writer = new TestPhysicalFileWriter(writeCallback: (_, _) => { }, flushCallback: () => flushCount++);
        var factory = new TestPhysicalFileWriterFactory(writer);

        var state = new UploadingFileState(CreateFileDto(totalFileParts: 2, partSize: 4, fileSize: 8),
                                          "unused",
                                          Guid.NewGuid(),
                                          factory);
        state.CloseEvent += (_, _) => closeEventCount++;

        var firstReader = PipeReader.Create(CreateSequence(payload));
        var firstResult = await state.WritePartFromPipeAsync(0, firstReader, CancellationToken.None, NullLogger<UploadingFileState>.Instance);
        Assert.IsType<Success<UploadPartSuccess>>(firstResult);
        Assert.Equal(1, state.PartsWritten);
        Assert.Equal(0, flushCount);
        Assert.Equal(0, closeEventCount);

        var secondReader = PipeReader.Create(CreateSequence(payload));
        var secondResult = await state.WritePartFromPipeAsync(1, secondReader, CancellationToken.None, NullLogger<UploadingFileState>.Instance);
        Assert.IsType<Success<UploadPartSuccess>>(secondResult);
        Assert.Equal(2, state.PartsWritten);
        Assert.Equal(1, flushCount);
        Assert.Equal(1, closeEventCount);
    }

    [Fact]
    public async Task WritePartFromPipeAsync_ReturnsFailureWhenWriterThrowsAndCompletesReader()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(payload);
        await pipe.Writer.CompleteAsync();

        var writer = new TestPhysicalFileWriter((_, _) => throw new InvalidOperationException("boom"));
        var factory = new TestPhysicalFileWriterFactory(writer);

        var state = new UploadingFileState(CreateFileDto(totalFileParts: 1, partSize: 4, fileSize: 4),
                                          "unused",
                                          Guid.NewGuid(),
                                          factory);

        var result = await state.WritePartFromPipeAsync(0, pipe.Reader, CancellationToken.None, NullLogger<UploadingFileState>.Instance);
        Assert.IsType<Failure<UploadPartSuccess>>(result);
    }
    [Fact]
    public async Task WritePartFromPipeAsync_MarksReceievedPartsAsDoneInBitfield()
    {

        var segments = new[]
        {
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6, 7, 8 }
        };

        var writer = new TestPhysicalFileWriter();
        var factory = new TestPhysicalFileWriterFactory(writer);

        var state = new UploadingFileState(CreateFileDto(totalFileParts: 125, partSize: 1024, fileSize: 125 * 1024),
                                          "unused",
                                          Guid.NewGuid(),
                                          factory);

        var sequence = CreateSequence(segments);

        var tasks = new Task<Result<UploadPartSuccess>>[3];

        tasks[0] = state.WritePartFromPipeAsync(1, PipeReader.Create(sequence), CancellationToken.None, NullLogger<UploadingFileState>.Instance);
        tasks[1] = state.WritePartFromPipeAsync(5, PipeReader.Create(sequence), CancellationToken.None, NullLogger<UploadingFileState>.Instance);
        tasks[2] = state.WritePartFromPipeAsync(8, PipeReader.Create(sequence), CancellationToken.None, NullLogger<UploadingFileState>.Instance);


        var finished = await Task.WhenAll(tasks);

        foreach (var (index, res) in finished.Index())
        {
            Assert.True(res is Success<UploadPartSuccess>, $"Task index: {index} Error: {(res as Failure<UploadPartSuccess>)?.Error}");
        }
        var part1 = 1u << 1;
        var part5 = 1u << 5;
        var part8 = 1u << 8;

        Assert.True((state.PartsBitfield & part1) != 0, "Bit [1] should be set");
        Assert.True((state.PartsBitfield & part5) != 0, "Bit [5] should be set");
        Assert.True((state.PartsBitfield & part8) != 0, "Bit [8] should be set");
    }

    [Fact]
    public async Task WritePartFromPipeAsync_RecievedPartWindowIsShiftedForSiquentialParts()
    {

        var segments = new[]
        {
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6, 7, 8 }
        };

        var writer = new TestPhysicalFileWriter();
        var factory = new TestPhysicalFileWriterFactory(writer);

        var state = new UploadingFileState(CreateFileDto(totalFileParts: 125, partSize: 1024, fileSize: 125 * 1024),
                                          "unused",
                                          Guid.NewGuid(),
                                          factory);

        var sequence = CreateSequence(segments);

        var tasks = new Task<Result<UploadPartSuccess>>[3];
        long initialStart = state.WindowStart;

        tasks[2] = state.WritePartFromPipeAsync(8, PipeReader.Create(sequence), CancellationToken.None, NullLogger<UploadingFileState>.Instance);

        tasks[0] = state.WritePartFromPipeAsync(0, PipeReader.Create(sequence), CancellationToken.None, NullLogger<UploadingFileState>.Instance);
        tasks[1] = state.WritePartFromPipeAsync(1, PipeReader.Create(sequence), CancellationToken.None, NullLogger<UploadingFileState>.Instance);


        var finished = await Task.WhenAll(tasks);

        foreach (var (index, res) in finished.Index())
        {
            Assert.True(res is Success<UploadPartSuccess>, $"Task index: {index} Error: {(res as Failure<UploadPartSuccess>)?.Error}");
        }

        var mask = 1u << 6;

        Assert.Equal(initialStart + 2, state.WindowStart);
        Assert.Equal(mask, state.PartsBitfield & mask);

    }
    private static FileCreationDto CreateFileDto(long totalFileParts, int partSize, long fileSize)
    {
        var randomStr = Generators.RandomString32();
        return new FileCreationDto("test.txt", fileSize, totalFileParts, partSize, 1, randomStr);
    }

    private static ReadOnlySequence<byte> CreateSequence(params byte[][] segments)
    {
        if (segments.Length == 0)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        var first = new SequenceSegment(segments[0]);
        var last = first;

        for (int i = 1; i < segments.Length; i++)
        {
            last = last.Append(segments[i]);
        }

        return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
    }

    private sealed class SequenceSegment : ReadOnlySequenceSegment<byte>
    {
        public SequenceSegment(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
        }

        public SequenceSegment Append(ReadOnlyMemory<byte> memory)
        {
            var segment = new SequenceSegment(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = segment;
            return segment;
        }
    }

    private sealed class TestPhysicalFileWriter : IPhysicalFileWriter
    {
        private readonly Action<ReadOnlyMemory<byte>, long>? _writeCallback;
        private readonly Action? _flushCallback;

        public bool IsClosed { get; set; }

        public TestPhysicalFileWriter(Action<ReadOnlyMemory<byte>, long>? writeCallback = null,
                                      Action? flushCallback = null)
        {
            _writeCallback = writeCallback;
            _flushCallback = flushCallback;
            IsClosed = false;
        }

        public ValueTask Write(ReadOnlyMemory<byte> blob, long offset, CancellationToken ct)
        {
            _writeCallback?.Invoke(blob, offset);
            return ValueTask.CompletedTask;
        }

        public void FlushToDisk()
        {
            _flushCallback?.Invoke();
        }

        public void Dispose()
        {
            IsClosed = true;
        }
    }

    private sealed class TestPhysicalFileWriterFactory : IPhysicalFileWriterFactory
    {
        private readonly IPhysicalFileWriter _writer;

        public TestPhysicalFileWriterFactory(IPhysicalFileWriter writer)
        {
            _writer = writer;
        }

        public IPhysicalFileWriter Create(string filePath, long preallocationSize)
        {
            return _writer;
        }
    }
}
