using System;
using System.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Win32.SafeHandles;
using mvc_server.Interfaces;

namespace mvc_server.Models;

public class StreamedFile : IStreamedFile
{
    private int _partsWritten = 0;

    public string Id { get; init; }
    public long FileSize { get; init; }
    public int TotalFileParts { get; init; }
    public string FileName { get; init; }
    public long PartSize { get; init; }
    public SafeFileHandle Stream { get; init; }
    public DateTime Created { get; init; }
    public int PartsWritten
    {
        get => _partsWritten;
        set
        {
            _partsWritten = value;
            if (value == TotalFileParts)
            {
                OnClose();
            }
        }
    }
    public event EventHandler<string>? CloseEvent;
    private void OnClose()
    {
        Stream.Close();
        CloseEvent?.Invoke(this, Id);
    }
}
