namespace core.Models;

public record FilePartDto(Guid Id, int CurrentPart, int BytesRead, Stream Data);
