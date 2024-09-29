using System;
using Microsoft.Win32.SafeHandles;

namespace mvc_server.Interfaces;

public interface IFileHandleProvider
{
    public SafeFileHandle FileHandle { get; }
    public void Close();

}
