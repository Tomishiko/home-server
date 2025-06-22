using System;
using web.Interfaces;
using Microsoft.Win32.SafeHandles;
using web.Models;
using core.Models;

namespace web.Interfaces;

public interface IStreamedFile
{
    string Id { get; }
    ulong FileSize { get; }
    uint TotalFileParts { get; }
    string FileName { get; }
    uint OwnerId { get; }
    uint PartSize { get; }
    SafeFileHandle GetFileHandle { get; }
    void Close();
    DateTime Created { get; }
    event EventHandler<CloseFileEventArgs>? CloseEvent;
    uint PartsWritten { get; set; }
    void IncrementPartsWrittenLocked();



}
