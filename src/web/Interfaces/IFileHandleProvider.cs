using System;
using Microsoft.Win32.SafeHandles;

namespace web.Interfaces;

public interface IFileHandleProvider
{
    public SafeFileHandle FileHandle { get; }
    public void Close();
    public bool IsClosed { get; }

}
