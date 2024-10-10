using System.Drawing;
namespace mvc_server.Models;


public class FileSystemEntry
{
    public FileInfo[]? Files { get; set; }
    public DateTime? Updated { get; set; }

}

