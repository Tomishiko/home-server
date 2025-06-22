namespace core.Services;
using Data.Shared;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using core.Models;
using Data.Core;

public class FileService : BaseDataService, IFileService
{
    public FileService(ApplicationDbContext context) : base(context) { }

    public async Task StageFileRecord(core.Models.File file,uint owner_id)
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
            UUID = file.UUID,
            Ext = file.Ext,
            Size = file.Size,
            Name = file.Name,
            owner_id = owner_id
        };
        await _context.Files.AddAsync(fileEnt);

    }

    public IAsyncEnumerable<File> GetFilesAsync()
    {
        return _context.Files
            .Include("Owner")
            .Select(f => new File(f.UUID, f.Name, f.Size, f.Ext, f.Owner.Uname))
            .AsAsyncEnumerable();
    }
}
