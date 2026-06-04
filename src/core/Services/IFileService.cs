using core.Models;

namespace core.Services;

public interface IFileService
{

    public void StageNewFileRecord(string UUID,
                                   string ext,
                                   string fileName,
                                   long fileSize,
                                   long owner_id,
                                   bool shared);
    public IAsyncEnumerable<FileMeta> GetSharedFilesAsync(CancellationToken ct);
    public IAsyncEnumerable<FileMeta> GetPrivateFilesAsync(long onwer_id, CancellationToken ct);
    public Task<FileMeta?> RequestFileAsync(long? userid, long fileid);
    public Task<int> SaveChangesAsync();
    public Task<int> MarkAsDeletedAsync(long userId, long fileId);
    //public IAsyncEnumerable<string> GetDeletedFiles();
    public void StageDeletion(long fileId);
}
