using Microsoft.AspNetCore.Mvc;
using core.Services;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly ICoreFS _coreFs;
    private readonly ILogger<FileController> _logger;

    public FileController(ICoreFS coreFS, ILogger<FileController> logger)
    {
        _coreFs = coreFS;
        _logger = logger;
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
    [Authorize]
    public IActionResult GetFile(int id)
    {
        var files = _coreFs.GetIndexFiles;
        var file = files.ToArray()[id];

        var fs = file.OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: file.Name);
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


}
