using Microsoft.Win32.SafeHandles;
using core.Interfaces;
using System.Buffers;

namespace core.Models;


public sealed class PhysicalFileWriter : IPhysicalFileWriter
{
    private readonly uint _totalFileParts;
    private readonly SafeFileHandle _fileHandleProvider;
    private uint _partsWritten = 0;
    private bool _isDisposed = false;

    public Guid Id { get; }
    public long FileSize { get; }
    public uint PartSize { get; }
    public string FileName { get; }
    public long OwnerId { get; }
    public bool IsClosed { get => _fileHandleProvider.IsClosed; }
    public DateTime Created { get; }


    public async Task WritePartAsync(Stream incomingData, int size, int currentPart)
    {
        ArgumentNullException.ThrowIfNull(incomingData);

        using var memOwner = MemoryPool<byte>.Shared.Rent(size);
        Memory<byte> buffer = memOwner.Memory[..size];
        await incomingData.ReadExactlyAsync(buffer);
        await RandomAccess.WriteAsync(_fileHandleProvider, buffer, currentPart * PartSize);
        IncrementPartsWrittenLocked();
    }

    public void IncrementPartsWrittenLocked()
    {
        Interlocked.Increment(ref _partsWritten);

        if (_partsWritten == _totalFileParts)
        {
            Close();
        }
    }

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
                        FileOptions.Asynchronous | FileOptions.RandomAccess | FileOptions.WriteThrough,
                        preallocationSize: fileDto.FileSize);


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
