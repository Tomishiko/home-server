namespace web.Controllers;

using Microsoft.AspNetCore.Mvc;
using core.Services;
using web.Helpers;
using System.Diagnostics;


[ApiController]
[Route("api")]
public class FileController : ControllerBase
{
    private readonly ICoreFS _coreFs;
    private readonly ILogger<FileController> _logger;
    private readonly IFileService _fileService;
    private readonly FileUploadHelperService _upload;

    public FileController(ICoreFS coreFS, ILogger<FileController> logger, IFileService fileService, FileUploadHelperService upload)
    {
        _coreFs = coreFS;
        _logger = logger;
        _fileService = fileService;
        _upload = upload;
    }
    [HttpGet("file/{id}")]
    public async Task<IActionResult> GetFile(uint id)
    {

        uint? userId = Utility.TryGetUserId(User);

        var fileRec = await _fileService.RequestFileAsync(userId, id);
        if (fileRec is null)
            return Forbid();
        return _upload.ServeFile(this, fileRec);
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
        uint? userId = Utility.TryGetUserId(User);
        if (userId is null) return Forbid("Unable to identify user");
        int deleteCount = await _fileService.MarkAsDeletedAsync((uint)userId, id);
        if (deleteCount == 0)
            return Forbid("File does not exist or insufficient rights");
        return Ok();
    }


}
