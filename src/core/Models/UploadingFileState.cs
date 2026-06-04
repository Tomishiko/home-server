using core.Interfaces;
using System.Buffers;
using System.IO.Pipelines;
using Microsoft.Extensions.Logging;
using core.Models.Generic;
using core.Domain;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace core.Models;


public sealed class UploadingFileState : IUploadingFileState
{

    //private readonly SafeFileHandle _fileHandleProvider;
    private const int _windowSize = 32;
    private readonly IPhysicalFileWriter _writer;
    private readonly Lock _syncObject = new();

    // "Bitfield" to represent file parts recieved, 32 bit sliding window
    private uint _partsBitfield = 0;
    private long _windowStart = 0;

    private bool _isDisposed = false;
    private int _partsWritten = 0;

    public uint PartsBitfield { get => _partsBitfield; }
    public long WindowStart { get => _windowStart; }
    public string FileFingerprint { get; } //32 bytes
    public int PartsWritten { get => _partsWritten; }
    public long TotalFileParts { get; }
    public bool IsDirty { get; private set; }
    public Guid Uuid { get; }
    public long FileSize { get; }
    public int PartSize { get; }
    public string FileName { get; }
    public long OwnerId { get; }
    public bool IsClosed { get => _writer.IsClosed; }

    public event EventHandler<CloseFileEventArgs>? CloseEvent;

    public UploadingFileState(FileCreationDto fileDto,
                              string storagePath,
                              Guid UUID,
                              IPhysicalFileWriterFactory physicalFileWriterFactory)
    {
        Uuid = UUID;
        TotalFileParts = fileDto.TotalFileParts;
        FileName = fileDto.FileName;
        OwnerId = fileDto.OwnerId;
        FileSize = fileDto.FileSize;
        PartSize = fileDto.PartSize;
        _writer = physicalFileWriterFactory
            .Create(Path.Combine(storagePath, UUID.ToString()), fileDto.FileSize);

        FileFingerprint = fileDto.Fingerprint;
    }



    public async Task<Result<UploadPartSuccess>> WritePartFromPipeAsync(int currentPart,
                                                                        PipeReader reader,
                                                                        CancellationToken ct,
                                                                        ILogger logger)
    {

        const int writeBufferSize = 128 * 1024;
        long currentOffset = PartSize * currentPart;

        int relativePosition = (int)(currentPart - _windowStart);
        if (relativePosition < 0 || relativePosition >= _windowSize)
        {
            return new Error("Provided part is outside of window bounds", 400);
        }

        var mask = 1u << relativePosition;
        if ((mask & _partsBitfield) != 0)
        {
            return new UploadPartSuccess(currentPart, PartSize);
        }

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


                            // If the buffer is full - write to disk
                            if (writeBufferIndex == writeBufferSize)
                            {
                                //await RandomAccess.WriteAsync(_fileHandleProvider,
                                //    writeBuffer.AsMemory(0, writeBufferSize),
                                //    currentOffset, ct);
                                await _writer.Write(writeBuffer.AsMemory(0, writeBufferSize),
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
                        //await RandomAccess.WriteAsync(_fileHandleProvider,
                        //    writeBuffer.AsMemory(0, writeBufferIndex),
                        //    currentOffset, ct);
                        await _writer.Write(writeBuffer.AsMemory(0, writeBufferIndex),
                                      currentOffset, ct);
                    }
                    break;
                }
            }

            MarkPartAsDone(mask);


            return new UploadPartSuccess(currentPart, totalBytes);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Upload part {part} was canceled.", currentPart);
            return new Error("Operation canceled");

        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Pipe reader state machine desynchronized for part {part}.", currentPart);
            return new Error("Internal processing stream corruption.", 500);
        }
        catch (ArgumentException ex)
        {
            logger.LogCritical(ex, "Invalid buffer memory boundary detected during AdvanceTo for part {part}.", currentPart);
            return new Error("Memory boundary corruption.", 500);
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "I/O or storage failure writing part {part}.", currentPart);
            return new Error("Storage or network failure during upload.", 507);
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


    public void Dispose()// TODO: better implementation of disposable maybe
    {
        if (_isDisposed) return;

        _writer?.Dispose();
        CloseEvent = null;
        _isDisposed = true;
    }

    public bool TryGetSnapshotBackup([NotNullWhen(true)] out FileUploadStateBackupContext? backupTask)
    {

        lock (_syncObject)
        {
            if (!IsDirty)
            {
                backupTask = null;
                return false;
            }

            backupTask = new FileUploadStateBackupContext
            {
                PartsWritten = _partsWritten,
                Bitfield = _partsBitfield,
                Id = Uuid
            };
            IsDirty = false;
            return true;
        }

    }

    private void Close()
    {
        CloseEvent?.Invoke(this, new CloseFileEventArgs(Uuid, FileName,
                    FileSize, DateTime.Now));
    }

    private void MarkPartAsDone(uint mask)
    {

        lock (_syncObject)
        {
            _partsBitfield |= mask;
            _partsWritten++;

            // shift bits for parts already recieved in order
            int trailingOnes = BitOperations.TrailingZeroCount(~_partsBitfield);

            if (trailingOnes > 0)
            {
                _partsBitfield = _partsBitfield >>> trailingOnes;// Always zero-fills the left side
                _windowStart += trailingOnes;
            }

            IsDirty = true;
        }


        if (_partsWritten == TotalFileParts)
        {
            _writer.FlushToDisk();
            Close();
        }

    }
}
