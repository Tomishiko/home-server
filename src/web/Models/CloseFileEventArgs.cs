namespace web.Models;

public class CloseFileEventArgs : EventArgs
{
    public string FileId { get; init; }
    public string FileName { get; init; }
    public ulong FileSize { get; init; }
    public DateTime ClosedAt { get; init; }
    public CloseFileEventArgs(string fileId, string fileName, ulong fileSize, DateTime closedAt)
    {
        FileId = fileId;
        FileName = fileName;
        FileSize = fileSize;
        ClosedAt = closedAt;
    }
}
