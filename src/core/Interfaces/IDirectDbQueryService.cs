using core.Domain;

namespace core.Interfaces;

public interface IDirectDbQuery
{
    ValueTask UpdateFileUploadStateLegacy(ReadOnlySpan<FileStateBackupContextLegacy> batch);

    Task<int> UpdateFileUploadState(ReadOnlyMemory<FileUploadStateBackupContext> batch,
                                            CancellationToken ct);
}
