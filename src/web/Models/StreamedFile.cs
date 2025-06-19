namespace web.Models;

using System;
using core.Models;
using Microsoft.Win32.SafeHandles;
using web.Interfaces;


public class StreamedFile : IStreamedFile
{
    private uint _partsWritten = 0;
    public IFileHandleProvider fileHandleProvider { private get; init; }
    public string Id { get; init; }
    public ulong FileSize { get; init; }
    public uint TotalFileParts { get; init; }
    public string FileName { get; init; }
    public uint PartSize { get; init; }
    public required uint OwnerId { get; init; }

    public SafeFileHandle GetFileHandle { get => fileHandleProvider.FileHandle; }
    public bool IsClosed { get => fileHandleProvider.IsClosed; }
    public DateTime Created { get; init; }
    public uint PartsWritten
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
    public event EventHandler<CloseFileEventArgs>? CloseEvent;

    public void Close()
    {
        fileHandleProvider.Close();
        CloseEvent?.Invoke(this, new CloseFileEventArgs(Id, FileName, FileSize, DateTime.Now));
    }

}
