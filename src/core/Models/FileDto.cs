namespace core.Models;

public record FileCreationDto(
        string FileName,
        long FileSize,
        int TotalFileParts,
        int PartSize,
        long OwnerId);
