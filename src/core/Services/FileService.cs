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

    public async Task NewFileRecordAsync(string UUID, string ext, string fileName, ulong fileSize, uint owner_id,bool shared)
    {
        //uint owner_id = await _userRepo.Query().Where(u => u.Uname == file.Owner).Select(u => u.Id).SingleAsync();
        //{
        //var fileEnt = new FileEntity
        //    UUID = file.UUID,
        //    Ext = file.Ext,
        //    Size = file.Size,
        //    Name = file.Name,
        //    owner_id = owner_id
        //};
        //await _fileRepo.AddAsync(fileEnt);
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
    private IAsyncEnumerable<File> GetFilesAsync(bool shared, uint? owner_id = null)
    {
        if (shared)
            return _context.Files
                .Include("Owner")
                .Where(f => f.Public)
                .Select(f => new File(f.UUID, f.Name, f.Size, f.Ext, f.Owner.Uname, f.Id))
                .AsAsyncEnumerable();
        else
        {
            Debug.Assert(owner_id is not null);
            return _context.Files
                .Include("Owner")
                .Where(f => f.Private && f.owner_id == owner_id)
                .Select(f => new File(f.UUID, f.Name, f.Size, f.Ext, f.Owner.Uname, f.Id))
                .AsAsyncEnumerable();

        }

    }
    public Task<FileEntity> GetFile(uint id)
    {
        return _context.Files.Where(f => f.Id == id).SingleAsync();
    }
    public IAsyncEnumerable<File> GetSharedFilesAsync()
    {
        return GetFilesAsync(true);
    }

    public IAsyncEnumerable<File> GetPrivateFilesAsync(uint owner_id)
    {
        return GetFilesAsync(false, owner_id);
    }
    public async Task<File?> RequestFileAsync(uint userId, uint fileId)
    {
        var fileInfo = await GetFile(fileId);

        if (fileInfo.Private && fileInfo.owner_id != userId)
            return null;

        return new File(fileInfo.UUID, fileInfo.Name, fileInfo.Size, fileInfo.Ext);
    }

}
