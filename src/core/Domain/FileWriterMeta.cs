using core.Models;

namespace core.Domain;

public record FileWriterMeta(long FileSize, int PartSize, string FileName,
        long OwnerId, long TotalFileParts)
{
    public FileWriterMeta(UploadingFileState state)
        : this(state.FileSize, state.PartSize, state.FileName,
                state.OwnerId, state.TotalFileParts)
    {
    }
}
