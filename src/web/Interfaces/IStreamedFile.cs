using System;
using web.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace web.Interfaces;

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
    void IncrementPartsWrittenLocked();


}
