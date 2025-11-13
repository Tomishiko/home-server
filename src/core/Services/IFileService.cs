namespace core.Services;
using core.Models;

public interface IFileService
{

    public Task NewFileRecordAsync(string UUID, string ext, string fileName, ulong fileSize, uint owner_id, bool shared);
    public IAsyncEnumerable<FileMeta> GetSharedFilesAsync();
    public IAsyncEnumerable<FileMeta> GetPrivateFilesAsync(uint onwer_id);
    public Task<FileMeta?> RequestFileAsync(uint? userid, uint fileid);
    public Task<int> SaveChangesAsync();
    public Task<int> MarkAsDeletedAsync(uint userId, uint fileId);
    //public IAsyncEnumerable<string> GetDeletedFiles();
    public void StageDeletion(uint fileId);
}
