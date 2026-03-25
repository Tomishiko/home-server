using System.IO.Pipelines;
using core.Models;
using core.Models.Generic;
using Microsoft.Extensions.Logging;


namespace core.Interfaces;

public interface IPhysicalFileWriter : IDisposable
{
    Guid Id { get; }
    long FileSize { get; }
    //uint TotalFileParts { get; }
    string FileName { get; }
    long OwnerId { get; }
    int PartSize { get; }
    //SafeFileHandle GetFileHandle { get; }
    DateTime Created { get; }
    event EventHandler<CloseFileEventArgs>? CloseEvent;
    //uint PartsWritten { get; }
    void IncrementPartsWrittenLocked();
    Task<Result<UploadPartSuccess>> WritePartAsync(Stream incomingData, int size, int currentPart,ILogger logger);
    Task<Result<UploadPartSuccess>> WritePartFromPipeAsync(int currentPart, PipeReader reader, CancellationToken ct, ILogger logger);

}
