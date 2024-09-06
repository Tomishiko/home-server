using System;

namespace mvc_server.Models;

public class StreamedFile
{
    public int TotalFileParts { get; set; }
    public int CurrentFilePart { get; set; }
    public string FileName { get; set; }
    public long PartSize { get; set; }
    public FileStream Stream { get; set; }

}
