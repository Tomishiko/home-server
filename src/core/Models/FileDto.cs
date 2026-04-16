namespace core.Models;

public record FileCreationDto(
        string FileName,
        long FileSize,
        long TotalFileParts,
        int PartSize,
        long OwnerId,
        byte[] Fingerprint);
