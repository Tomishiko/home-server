namespace core.Domain;

public record FileWriterMeta(
    long FileSize,
    int PartSize,
    string FileName,
    long OwnerId,
    long TotalFileParts
);
