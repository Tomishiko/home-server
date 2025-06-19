namespace core.Services;
using core.Models;

public interface IFileService
{

    public Task StageFileRecord(core.Models.File file,uint owner_id);
    public IAsyncEnumerable<File> GetFilesAsync();
    public Task<int> SaveChangesAsync();
}
