using core.Models;
using Microsoft.Win32.SafeHandles;


namespace core.Interfaces;

public interface IPhysicalFileWriter : IDisposable
{
    Guid Id { get; }
    long FileSize { get; }
    //uint TotalFileParts { get; }
    string FileName { get; }
    long OwnerId { get; }
    uint PartSize { get; }
    //SafeFileHandle GetFileHandle { get; }
    DateTime Created { get; }
    event EventHandler<CloseFileEventArgs>? CloseEvent;
    //uint PartsWritten { get; }
    void IncrementPartsWrittenLocked();
    Task WritePartAsync(Stream incomingData, int size, int currentPart);

}
