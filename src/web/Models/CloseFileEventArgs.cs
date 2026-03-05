namespace web.Models;

public class CloseFileEventArgs : EventArgs
{
    public string FileId { get; init; }
    public string FileName { get; init; }
    public long FileSize { get; init; }
    public DateTime ClosedAt { get; init; }
    public CloseFileEventArgs(string fileId,
                              string fileName,
                              long fileSize,
                              DateTime closedAt)
    {
        FileId = fileId;
        FileName = fileName;
        FileSize = fileSize;
        ClosedAt = closedAt;
    }
}
