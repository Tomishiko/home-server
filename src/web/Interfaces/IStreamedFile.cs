using Microsoft.Win32.SafeHandles;
using web.Models;

namespace web.Interfaces;

public interface IStreamedFile : IDisposable
{
    string Id { get; }
    long FileSize { get; }
    uint TotalFileParts { get; }
    string FileName { get; }
    long OwnerId { get; }
    uint PartSize { get; }
    SafeFileHandle GetFileHandle { get; }
    DateTime Created { get; }
    event EventHandler<CloseFileEventArgs>? CloseEvent;
    uint PartsWritten { get; }
    void IncrementPartsWrittenLocked();

}
