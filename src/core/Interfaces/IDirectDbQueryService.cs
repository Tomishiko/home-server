using core.Domain;

namespace core.Interfaces;

public interface IDirectDbQueryService
{
    ValueTask UpdateFileUploadState(ReadOnlySpan<FileStateBackupContext> batch);
}
