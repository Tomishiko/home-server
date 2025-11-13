namespace core.Services;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using core.Models;
using Data.Core;
using System.Diagnostics;


public class FileService : BaseDataService, IFileService
{
    public FileService(ApplicationDbContext context) : base(context) { }

    public async Task NewFileRecordAsync(string UUID, string ext, string fileName, ulong fileSize, uint owner_id, bool shared)
    {
        var fileEnt = new FileEntity
        {
            UUID = UUID,
            Ext = ext,
            Size = fileSize,
            Name = fileName,
            Public = shared,
            owner_id = owner_id
        };
        await _context.Files.AddAsync(fileEnt);

    }
    ///<summary>If <paramref name="shared"/> is false
    ///<paramref name="owner_id"/> should be specified
    ///</summary>
    ///<exception cref="ArgumentNullException"> Throws if shared is false
    ///and owner id is not provided or null</exception>
    private IAsyncEnumerable<FileMeta> GetFiles(bool shared, uint? owner_id = null)
    {
        if (shared)
            return _context.Files
                .Include("Owner")
                .Where(f => f.Public && !f.IsDeleted)
                .Select(f => new FileMeta(f.UUID, f.Name, f.Size, f.Ext, f.Owner.Uname, f.Id))
                .AsAsyncEnumerable();
        else
        {
            Debug.Assert(owner_id is not null);
            return _context.Files
                .Include("Owner")
                .Where(f => f.Private && f.owner_id == owner_id && !f.IsDeleted)
                .Select(f => new FileMeta(f.UUID, f.Name, f.Size, f.Ext, f.Owner.Uname, f.Id))
                .AsAsyncEnumerable();

        }

    }
    public Task<FileEntity> GetFile(uint id)
    {
        return _context.Files.Where(f => f.Id == id && !f.IsDeleted).SingleAsync();
    }
    public IAsyncEnumerable<FileMeta> GetSharedFilesAsync()
    {
        return GetFiles(true);
    }

    public IAsyncEnumerable<FileMeta> GetPrivateFilesAsync(uint owner_id)
    {
        return GetFiles(false, owner_id);
    }
    public async Task<FileMeta?> RequestFileAsync(uint? userId, uint fileId)
    {
        var fileInfo = await GetFile(fileId);

        if (fileInfo.Private && (userId == null || fileInfo.owner_id != userId))
            return null;

        return new FileMeta(fileInfo.UUID, fileInfo.Name, fileInfo.Size, fileInfo.Ext);
    }

    ///<summary>
    /// Marks a file for a deletion
    ///</summary>
    ///<returns>Amount of records marked for deletion</returns>
    public Task<int> MarkAsDeletedAsync(uint userId, uint fileId)
    {
        return _context.Files
            .Where(f => f.Id == fileId && f.owner_id == userId)
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
    public void StageDeletion(uint fileId)
    {
        _context.Files.Remove(new FileEntity { Id = fileId });

    }

}
