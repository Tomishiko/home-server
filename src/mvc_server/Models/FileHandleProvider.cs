using System;
using Microsoft.Win32.SafeHandles;
using mvc_server.Interfaces;
namespace mvc_server.Models;


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
    }
}
