using System;
using mvc_server.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace mvc_server.Interfaces;

public interface IStreamedFile
{
    string Id { get; }
    long FileSize { get; }
    int TotalFileParts { get; }
    string FileName { get; }
    long PartSize { get; }
    SafeFileHandle GetFileHandle { get; }
    void Close();
    DateTime Created { get; }
    event EventHandler<string>? CloseEvent;
    int PartsWritten { get; set; }


}
