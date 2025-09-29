using Microsoft.AspNetCore.Mvc;
using core.Services;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace web.Controllers;

[ApiController]
[Route("api")]
public class FileController : ControllerBase
{
    private readonly ICoreFS _coreFs;
    private readonly ILogger<FileController> _logger;
    private readonly IFileService _fileService;

    public FileController(ICoreFS coreFS, ILogger<FileController> logger, IFileService fileService)
    {
        _coreFs = coreFS;
        _logger = logger;
        _fileService = fileService;
    }
    private uint? GetUserId()
    {
        uint userId;
        if (!uint.TryParse(User.FindFirst("Id")?.Value, out userId))
            return null;
        return userId;
    }
    [HttpGet("video/{id}")]
    public IActionResult GetVideo(int id)
    {
        var videos = _coreFs.GetMovies;
        var file = videos.ToArray()[id];
        var fs = file.OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: file.Name);
    }
    [HttpGet("file/{id}")]
    //[Authorize]
    public async Task<IActionResult> GetFile(uint id)
    {

        uint? userId = GetUserId();
        Console.WriteLine(userId);

        var fileRec = await _fileService.RequestFileAsync(userId, id);
        if (fileRec is null)
            return Forbid();

        var fs = _coreFs.GetFileStream(fileRec.UUID);
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: $"{fileRec.Name}.{fileRec.Ext}");
    }
    [HttpGet("pfile/{id}")]
    public async Task<IActionResult> PrintFile(int id, [FromQuery] string printParams)
    {
        var files = _coreFs.GetIndexFiles;
        var file = files.ToArray()[id];
        using Process cmd = new Process();
        cmd.StartInfo.FileName = "lp";
        cmd.StartInfo.Arguments = $"{file.FullName}";
        cmd.Start();
        await cmd.WaitForExitAsync();
        return Ok();
    }
    [HttpDelete("file/{id}")]
    public async Task<IActionResult> DeleteFile(uint id)
    {
        uint? userId = GetUserId();
        if (userId is null) return Forbid("Unable to identify user");
        int deleteCount = await _fileService.MarkAsDeletedAsync((uint)userId, id);
        if (deleteCount == 0)
            return Forbid("File does not exist or insufficient rights");
        return Ok();
    }


}
