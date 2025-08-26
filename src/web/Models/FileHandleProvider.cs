using Microsoft.Win32.SafeHandles;
using web.Interfaces;
namespace web.Models;


public class FileHandleProvider : IFileHandleProvider, IDisposable
{
    public SafeFileHandle FileHandle { get; }

    public bool IsClosed => FileHandle.IsClosed;

    public bool IsDisposed { get; private set; } = false;

    public FileHandleProvider(SafeFileHandle fileHandle)
    {
        this.FileHandle = fileHandle;
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        FileHandle.Dispose();
        GC.SuppressFinalize(true);
        IsDisposed = true;
    }
}
