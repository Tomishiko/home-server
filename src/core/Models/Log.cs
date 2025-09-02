namespace core.Models;

public record Log(uint Id,string Event, DateTime Time, string? Uname = null);
