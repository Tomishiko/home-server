using core.Models;

namespace core.Services;

public interface IFileService
{

    public Task NewFileRecordAsync(string UUID,
                                   string ext,
                                   string fileName,
                                   long fileSize,
                                   long owner_id,
                                   bool shared);
    public IAsyncEnumerable<FileMeta> GetSharedFilesAsync();
    public IAsyncEnumerable<FileMeta> GetPrivateFilesAsync(long onwer_id);
    public Task<FileMeta?> RequestFileAsync(long? userid, long fileid);
    public Task<int> SaveChangesAsync();
    public Task<int> MarkAsDeletedAsync(long userId, long fileId);
    //public IAsyncEnumerable<string> GetDeletedFiles();
    public void StageDeletion(long fileId);
}
