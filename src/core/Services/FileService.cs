using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using core.Interfaces;
using core.Domain;
using core.Models;
using System.Runtime.CompilerServices;

namespace core.Services;

public class FileService : BaseDataService, IFileService
{
    public FileService(IApplicationDbContext context) : base(context) { }

    public async Task StageNewFileRecord(string UUID, string ext, string fileName,
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
        _context.Files.Add(fileEnt);

    }
    ///<summary>If <paramref name="shared"/> is false
    ///<paramref name="owner_id"/> should be specified
    ///</summary>
    ///<exception cref="ArgumentNullException"> Throws if shared is false
    ///and owner id is not provided or null</exception>
    private async IAsyncEnumerable<FileMeta> GetFiles(bool shared,
                                                      long? owner_id = null,
                                                      [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (shared)
        {

            var stream = _context.Files
                .AsNoTracking()
                .Include("Owner")
                .Where(f => f.IsPublic && !f.IsDeleted)
                .AsAsyncEnumerable()
                .WithCancellation(ct);

            await foreach (FileEntity f in stream)
            {
                yield return new FileMeta(f.UUID, f.Name, f.Size, f.Ext,
                            f.Owner?.Uname, f.Id);
            }
        }
        else
        {
            Debug.Assert(owner_id is not null);
            var stream = _context.Files
                .AsNoTracking()
                .Include("Owner")
                .Where(f => !f.IsPublic && f.OwnerId == owner_id && !f.IsDeleted)
                .AsAsyncEnumerable()
                .WithCancellation(ct);

            await foreach (FileEntity f in stream)
            {
                yield return new FileMeta(f.UUID, f.Name, f.Size, f.Ext,
                            f.Owner?.Uname, f.Id);
            }

        }

    }
    public Task<FileEntity?> GetFile(long id)
    {
        return _context.Files
                       .Where(f => f.Id == id && !f.IsDeleted)
                       .SingleOrDefaultAsync();
    }
    public IAsyncEnumerable<FileMeta> GetSharedFilesAsync(CancellationToken ct = default)
    {
        return GetFiles(true, ct: ct);
    }

    public IAsyncEnumerable<FileMeta> GetPrivateFilesAsync(long owner_id, CancellationToken ct = default)
    {
        return GetFiles(false, owner_id, ct);
    }
    public async Task<FileMeta?> RequestFileAsync(long? userId, long fileId)
    {
        var fileInfo = await GetFile(fileId);
        if (fileInfo is null)
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
