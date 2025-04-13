namespace core.Models;

public record Log(string Event, DateTime Time, string? Uname = null);
