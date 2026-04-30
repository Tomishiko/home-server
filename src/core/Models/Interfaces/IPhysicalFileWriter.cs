using System.IO.Pipelines;
using core.Domain;
using core.Models;
using core.Models.Generic;
using Microsoft.Extensions.Logging;


namespace core.Interfaces;

public interface IUploadingFileState : IDisposable
{
    Guid Uuid { get; }
    long FileSize { get; }
    //uint TotalFileParts { get; }
    string FileName { get; }
    long OwnerId { get; }
    int PartSize { get; }
    //SafeFileHandle GetFileHandle { get; }
    event EventHandler<CloseFileEventArgs>? CloseEvent;
    //uint PartsWritten { get; }
    bool IsDirty { get; }
    byte[] FileFingerprint { get; }

    FileStateBackupContext GetSnapshot();
    Task<Result<UploadPartSuccess>> WritePartFromPipeAsync(int currentPart, PipeReader reader, CancellationToken ct, ILogger logger);

}
