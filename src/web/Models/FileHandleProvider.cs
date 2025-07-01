using Microsoft.Win32.SafeHandles;
using web.Interfaces;
namespace web.Models;


public class FileHandleProvider : IFileHandleProvider
{
    public SafeFileHandle FileHandle { get; }

    public bool IsClosed => FileHandle.IsClosed;

    public FileHandleProvider(SafeFileHandle fileHandle)
    {
        this.FileHandle = fileHandle;
    }

    public void Close()
    {
        FileHandle.Close();
        FileHandle.Dispose();
    }
}
