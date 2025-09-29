namespace core.Services;
using core.Models;

public interface IFileService
{

    public Task NewFileRecordAsync(string UUID, string ext, string fileName, ulong fileSize, uint owner_id, bool shared);
    public IAsyncEnumerable<File> GetSharedFilesAsync();
    public IAsyncEnumerable<File> GetPrivateFilesAsync(uint onwer_id);
    public Task<File?> RequestFileAsync(uint? userid, uint fileid);
    public Task<int> SaveChangesAsync();
    public Task<int> MarkAsDeletedAsync(uint userId, uint fileId);
    //public IAsyncEnumerable<string> GetDeletedFiles();
    public void StageDeletion(uint fileId);
}
