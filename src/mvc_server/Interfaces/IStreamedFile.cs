using System;
using Microsoft.Win32.SafeHandles;

namespace mvc_server.Interfaces;

public interface IStreamedFile
{
    string Id { get; init; }
    long FileSize { get; init; }
    int TotalFileParts { get; init; }
    string FileName { get; init; }
    long PartSize { get; init; }
    SafeFileHandle Stream { get; init; }
    DateTime Created { get; init; }
    event EventHandler<string>? CloseEvent;
    int PartsWritten { get; set; }


}
