namespace core.Models;

public sealed record LogDto(long Id,
                     string Event,
                     DateTimeOffset Time,
                     string? Uname = null);
