using core.Interfaces;
using System.Buffers;
using System.IO.Pipelines;
using Microsoft.Extensions.Logging;
using core.Models.Generic;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using core.Domain;

namespace core.Models;


public sealed class UploadingFileState : IUploadingFileState
{

    private readonly SafeFileHandle _fileHandleProvider;
    private readonly Lock _syncObject = new();

    // "Bitfield" to represent file parts recieved, 8 parts per byte
    private readonly byte[] _partsBitfield;

    private bool _isDisposed = false;
    private int _partsWritten = 0;

    public ReadOnlySpan<byte> PartsBitfield { get => _partsBitfield.AsSpan(); }
    public byte[] FileFingerprint { get; } //32 bytes
    public int PartsWritten { get => _partsWritten; }
    public long TotalFileParts { get; }
    public bool IsDirty { get; private set; }
    public Guid Id { get; }
    public long FileSize { get; }
    public int PartSize { get; }
    public string FileName { get; }
    public long OwnerId { get; }
    public bool IsClosed { get => _fileHandleProvider.IsClosed; }

    public event EventHandler<CloseFileEventArgs>? CloseEvent;

    public UploadingFileState(FileCreationDto fileDto, string storagePath, Guid UUID, byte[] fingerprint)
    {
        Id = UUID;
        TotalFileParts = fileDto.TotalFileParts;
        FileName = fileDto.FileName;
        OwnerId = fileDto.OwnerId;
        FileSize = fileDto.FileSize;
        PartSize = fileDto.PartSize;

        _partsBitfield = new byte[(TotalFileParts + 7) / 8]; // +7 for edge cases
        FileFingerprint = fingerprint;
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


                            // If the buffer is full - write to disk
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

            MarkPartAsDone(currentPart);


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


    public void Dispose()// TODO: better implementation of disposable maybe
    {
        if (_isDisposed) return;

        _fileHandleProvider?.Dispose();
        CloseEvent = null;
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public FileStateBackupContext GetSnapshot()
    {

        FileStateBackupContext backupTask;

        lock (_syncObject)
        {
            backupTask = new FileStateBackupContext(_partsWritten,
                                                     _partsBitfield,
                                                     Id);
        }

        return backupTask;
    }

    private void Close()
    {
        CloseEvent?.Invoke(this, new CloseFileEventArgs(Id, FileName,
                    FileSize, DateTime.Now));
    }

    private void MarkPartAsDone(int index)
    {
        int byteIdx = index / 8;
        byte mask = (byte)(1 << (index % 8));

        lock (_syncObject)
        {
            // Only increment if we haven't seen this part before
            if ((_partsBitfield[byteIdx] & mask) == 0)
            {
                _partsBitfield[byteIdx] |= mask;
                _partsWritten++;
                IsDirty = true;
            }
        }


        if (_partsWritten == TotalFileParts)
        {
            RandomAccess.FlushToDisk(_fileHandleProvider);
            Close();
        }

    }
}
