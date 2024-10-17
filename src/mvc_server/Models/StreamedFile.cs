using System;
using System.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Win32.SafeHandles;
using mvc_server.Interfaces;

namespace mvc_server.Models;

public class StreamedFile : IStreamedFile
{
    private int _partsWritten = 0;
    public IFileHandleProvider fileHandleProvider { private get; init; }
    public string Id { get; init; }
    public long FileSize { get; init; }
    public int TotalFileParts { get; init; }
    public string FileName { get; init; }
    public long PartSize { get; init; }

    public SafeFileHandle GetFileHandle { get => fileHandleProvider.FileHandle; }
    public bool IsClosed { get => fileHandleProvider.IsClosed; }
    public DateTime Created { get; init; }
    public int PartsWritten
    {
        get => _partsWritten;
        set
        {
            _partsWritten = value;
            if (value == TotalFileParts)
            {
                Close();
            }
        }
    }
    public void IncrementPartsWrittenLocked()
    {
        Interlocked.Increment(ref _partsWritten);
        if(_partsWritten == TotalFileParts)
        {
            Close();
        }
    }
    public event EventHandler<string>? CloseEvent;

    public void Close()
    {
        fileHandleProvider.Close();
        CloseEvent?.Invoke(this, Id);
    }

}
