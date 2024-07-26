namespace mvc_server.Models;

public class MovieModel
{
    public int SelectionId { get; set; }
    public IEnumerable<FileInfo> Movies { get; set; }
    public string? SelectionName { get; set; }
    public string? PlayerPath { get; set; }
}
