namespace core.Models;

public class Log
{
    int user_id { get; set; }
    public User User { get; set; }
    public string Event { get; set; }
    public DateTime Time { get; set; }
}
