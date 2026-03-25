using Microsoft.Win32.SafeHandles;
using core.Interfaces;
using System.Buffers;
using System.IO.Pipelines;
using Microsoft.Extensions.Logging;
using core.Models.Generic;

namespace core.Models;


public sealed class PhysicalFileWriter : IPhysicalFileWriter
{
    private readonly int _totalFileParts;
    private readonly SafeFileHandle _fileHandleProvider;
    private int _partsWritten = 0;
    private bool _isDisposed = false;

    public Guid Id { get; }
    public long FileSize { get; }
    public int PartSize { get; }
    public string FileName { get; }
    public long OwnerId { get; }
    public bool IsClosed { get => _fileHandleProvider.IsClosed; }
    public DateTime Created { get; }

    public PhysicalFileWriter(FileCreationDto fileDto, string storagePath, Guid UUID)
    {
        Id = UUID;
        _totalFileParts = fileDto.TotalFileParts;
        FileName = fileDto.FileName;
        OwnerId = fileDto.OwnerId;
        FileSize = fileDto.FileSize;
        PartSize = fileDto.PartSize;

        _fileHandleProvider = File.OpenHandle(
                        Path.Combine(storagePath, UUID.ToString()),
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.Write,
                        FileOptions.Asynchronous | FileOptions.RandomAccess, //| FileOptions.WriteThrough,
                        preallocationSize: fileDto.FileSize);


    }

    public async Task<Result<UploadPartSuccess>> WritePartAsync(Stream incomingData, int size, int currentPart, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(incomingData);

        try
        {

            using var memOwner = MemoryPool<byte>.Shared.Rent(size);
            Memory<byte> buffer = memOwner.Memory[..size];
            await incomingData.ReadExactlyAsync(buffer);
            await RandomAccess.WriteAsync(_fileHandleProvider, buffer, currentPart * PartSize);
            IncrementPartsWrittenLocked();
            return new UploadPartSuccess(currentPart, buffer.Length);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing file part {part}", currentPart);
            return new Error("Something unexpected happened", 500);
        }
    }

    public async Task<Result<UploadPartSuccess>> WritePartFromPipeAsync(int currentPart,
                                                                        PipeReader reader,
                                                                        CancellationToken ct,
                                                                        ILogger logger)
    {
        const int writeBufferSize = 128 * 1024;

        long currentOffset = PartSize * currentPart;

        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(writeBufferSize);
        int writeBufferIndex = 0;
        long totalBytes = 0;

        try
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync(ct);
                ReadOnlySequence<byte> buffer = result.Buffer;

                if (!buffer.IsEmpty)
                {
                    foreach (ReadOnlyMemory<byte> segment in buffer)
                    {
                        int segmentOffset = 0;
                        int remainingInSegment = segment.Length;

                        while (remainingInSegment > 0)
                        {

                            //Block for span to go out of scope before await
                            {
                                ReadOnlySpan<byte> segmentSpan = segment.Span;
                                int remainingInBuffer = writeBufferSize - writeBufferIndex;
                                int bytesToCopy = Math.Min(remainingInSegment, remainingInBuffer);

                                // Copy segment data into buffer
                                segmentSpan.Slice(segmentOffset, bytesToCopy)
                                           .CopyTo(writeBuffer.AsSpan(writeBufferIndex));
                                writeBufferIndex += bytesToCopy;
                                segmentOffset += bytesToCopy;
                                remainingInSegment -= bytesToCopy;
                                totalBytes += bytesToCopy;
                            }


                            // If the buffer is full, flush it to disk
                            if (writeBufferIndex == writeBufferSize)
                            {
                                await RandomAccess.WriteAsync(_fileHandleProvider,
                                    writeBuffer.AsMemory(0, writeBufferSize),
                                    currentOffset, ct);

                                currentOffset += writeBufferSize;
                                writeBufferIndex = 0;
                            }
                        }
                    }
                }

                reader.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    if (writeBufferIndex > 0)
                    {
                        await RandomAccess.WriteAsync(_fileHandleProvider,
                            writeBuffer.AsMemory(0, writeBufferIndex),
                            currentOffset, ct);
                    }
                    break;
                }
            }

            IncrementPartsWrittenLocked();
            return new UploadPartSuccess(currentPart, totalBytes);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing pipe segments for part {part}", currentPart);
            return new Error("Some unexpected error happened", 500);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(writeBuffer);
            await reader.CompleteAsync();
        }

    }

    public void IncrementPartsWrittenLocked()
    {
        Interlocked.Increment(ref _partsWritten);

        if (_partsWritten == _totalFileParts)
        {
            RandomAccess.FlushToDisk(_fileHandleProvider);
            Close();
        }
    }


    public event EventHandler<CloseFileEventArgs>? CloseEvent;

    void Close()
    {
        CloseEvent?.Invoke(this, new CloseFileEventArgs(Id, FileName,
                    FileSize, DateTime.Now));
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _fileHandleProvider?.Dispose();
        GC.SuppressFinalize(this);
        CloseEvent = null;
        _isDisposed = true;
    }
}
