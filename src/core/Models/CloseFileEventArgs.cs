namespace core.Models;

public class CloseFileEventArgs : EventArgs
{
    public Guid FileId { get; init; }
    public string FileName { get; init; }
    public long FileSize { get; init; }
    public DateTime ClosedAt { get; init; }

    public CloseFileEventArgs(Guid fileId,
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
