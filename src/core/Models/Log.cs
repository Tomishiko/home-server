namespace core.Models;

public class Log
{
    public User User { get; set; } = null!;
    public string Event { get; set; }
    public DateTime Time { get; set; }
}
