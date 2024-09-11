using System;
using Microsoft.Win32.SafeHandles;

namespace mvc_server.Models;

public class StreamedFile
{
    public int TotalFileParts { get; set; }
    public int CurrentFilePart { get; set; }
    public string FileName { get; set; }
    public long PartSize { get; set; }
    public SafeFileHandle Stream { get; set; }
    public DateTime Created { get; init; }

}
