using Microsoft.Win32.SafeHandles;
using web.Models;

namespace web.Interfaces;

public interface IStreamedFile : IDisposable
{
    string Id { get; }
    ulong FileSize { get; }
    uint TotalFileParts { get; }
    string FileName { get; }
    uint OwnerId { get; }
    uint PartSize { get; }
    SafeFileHandle GetFileHandle { get; }
    DateTime Created { get; }
    event EventHandler<CloseFileEventArgs>? CloseEvent;
    uint PartsWritten { get; }
    void IncrementPartsWrittenLocked();

}
