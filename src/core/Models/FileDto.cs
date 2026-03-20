namespace core.Models;

public record FileCreationDto(
        string FileName,
        long FileSize,
        uint TotalFileParts,
        uint PartSize,
        long OwnerId);
