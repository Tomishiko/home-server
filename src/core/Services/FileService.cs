using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using core.Interfaces;
using core.Domain;
using core.Models;

namespace core.Services;

public class FileService : BaseDataService, IFileService
{
    public FileService(IApplicationDbContext context) : base(context) { }

    public async Task NewFileRecordAsync(string UUID, string ext, string fileName,
            long fileSize, long owner_id, bool shared) // TODO: make a dto for this crap ffs
    {
        var fileEnt = new FileEntity
        {
            UUID = UUID,
            Ext = ext,
            Size = fileSize,
            Name = fileName,
            IsPublic = shared,
            OwnerId = owner_id
        };
        await _context.Files.AddAsync(fileEnt);

    }
    ///<summary>If <paramref name="shared"/> is false
    ///<paramref name="owner_id"/> should be specified
    ///</summary>
    ///<exception cref="ArgumentNullException"> Throws if shared is false
    ///and owner id is not provided or null</exception>
    private IAsyncEnumerable<FileMeta> GetFiles(bool shared, long? owner_id = null)
    {
        if (shared)
            return _context.Files
                .Include("Owner")
                .Where(f => f.IsPublic && !f.IsDeleted)
                .Select(f => new FileMeta(f.UUID, f.Name, f.Size, f.Ext,
                            f.Owner.Uname, f.Id))
                .AsAsyncEnumerable();
        else
        {
            Debug.Assert(owner_id is not null);
            return _context.Files
                .Include("Owner")
                .Where(f => f.IsPrivate && f.OwnerId == owner_id && !f.IsDeleted)
                .Select(f => new FileMeta(f.UUID, f.Name, f.Size, f.Ext,
                            f.Owner.Uname, f.Id))
                .AsAsyncEnumerable();

        }

    }
    public Task<FileEntity?> GetFile(long id)
    {
        return _context.Files.Where(f => f.Id == id && !f.IsDeleted).SingleOrDefaultAsync();
    }
    public IAsyncEnumerable<FileMeta> GetSharedFilesAsync()
    {
        return GetFiles(true);
    }

    public IAsyncEnumerable<FileMeta> GetPrivateFilesAsync(long owner_id)
    {
        return GetFiles(false, owner_id);
    }
    public async Task<FileMeta?> RequestFileAsync(long? userId, long fileId)
    {
        var fileInfo = await GetFile(fileId);
        if(fileInfo is null)
        {
            return null;
        }

        if (fileInfo.IsPrivate && (userId == null || fileInfo.OwnerId != userId))
            return null;

        return new FileMeta(fileInfo.UUID, fileInfo.Name, fileInfo.Size, fileInfo.Ext);
    }

    ///<summary>
    /// Marks a file for a deletion
    ///</summary>
    ///<returns>Amount of records marked for deletion</returns>
    public Task<int> MarkAsDeletedAsync(long userId, long fileId)
    {
        return _context.Files
            .Where(f => f.Id == fileId && f.OwnerId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsDeleted, true));

    }
    ///<summary>
    ///Returns a list of file names which are flagged deleted in DB
    ///</summary>
    // public IAsyncEnumerable<File> GetDeletedFiles()
    // {
    //     return _context.Files.Where(f => f.IsDeleted)
    //                          .Select(f=>new File("",$"wwwroot/files/{f.UUID}",))
    //                          .AsAsyncEnumerable();
    // }
    public void StageDeletion(long fileId)
    {
        _context.Files.Remove(new FileEntity { Id = fileId });

    }

}
