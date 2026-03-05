using Microsoft.Win32.SafeHandles;
using web.Interfaces;

namespace web.Models;


public sealed class StreamedFile : IStreamedFile
{
    private uint _partsWritten = 0;
    private SafeFileHandle fileHandleProvider;
    private bool _isDisposed = false;

    public string Id { get; init; }
    public long FileSize { get; init; }
    public uint TotalFileParts { get; init; }
    public string FileName { get; init; }
    public uint PartSize { get; init; }
    public long OwnerId { get; init; }
    public SafeFileHandle GetFileHandle { get => fileHandleProvider; }
    public bool IsClosed { get => fileHandleProvider.IsClosed; }
    public DateTime Created { get; init; }
    public uint PartsWritten
    {
        get => _partsWritten;
    }
    public void IncrementPartsWrittenLocked()
    {
        Interlocked.Increment(ref _partsWritten);
        if (_partsWritten == TotalFileParts)
        {
            Close();
        }
    }
    public event EventHandler<CloseFileEventArgs>? CloseEvent;

    public StreamedFile(FileHandleConfig fconfig, string uuid, uint totalFileParts,
            string fileName, uint partSize, long ownerId)
    {
        Id = uuid;
        TotalFileParts = totalFileParts;
        FileName = fileName;
        PartSize = partSize;
        OwnerId = ownerId;
        FileSize = fconfig.preallocationSize;
        fileHandleProvider = File.OpenHandle(fconfig.path,
                                            fconfig.fileMode,
                                            fconfig.FileAccess,
                                            fconfig.fileShare,
                                            FileOptions.Asynchronous | FileOptions.RandomAccess | FileOptions.WriteThrough,
                                            preallocationSize: fconfig.preallocationSize);
    }


    void Close() => CloseEvent?.Invoke(this, new CloseFileEventArgs(Id, FileName, FileSize, DateTime.Now));

    public void Dispose()
    {
        if (_isDisposed) return;

        fileHandleProvider?.Dispose();
        GC.SuppressFinalize(this);
        CloseEvent = null;
        _isDisposed = true;
    }
}
