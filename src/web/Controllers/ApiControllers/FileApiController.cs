using Microsoft.AspNetCore.Mvc;
using core.Services;
using web.Helpers;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using core.Models;

namespace web.Controllers;


[ApiController]
[Route("api")]
public class FileApiController : ControllerBase
{
    private readonly ILogger<FileApiController> _logger;
    private readonly IFileService _fileService;
    private readonly FileUploadHelperService _upload;

    public FileApiController(ILogger<FileApiController> logger,
                          IFileService fileService,
                          FileUploadHelperService upload)
    {
        _logger = logger;
        _fileService = fileService;
        _upload = upload;
    }

    [HttpGet("file/{id}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "aplication/octet-stream")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(long id)
    {

        long? userId = Utility.TryGetUserId(User);

        var fileRec = await _fileService.RequestFileAsync(userId, id);
        if (fileRec is null)
            return NotFound();

        return _upload.ServeFile(this, fileRec);
    }

    [HttpGet("files")]
    public async IAsyncEnumerable<FileMeta> GetFiles()
    {

        long? userId = Utility.TryGetUserId(User);

        var enumerable = _fileService.GetSharedFilesAsync(CancellationToken.None);
        await foreach (var item in enumerable)
        {
            yield return item;
        }

    }

    [HttpDelete("file/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile(long id) //TODO: use UUID here
    {
        long? userId = Utility.TryGetUserId(User);
        if (userId is null)
        {
            return Forbid("Unable to identify user");
        }

        int deleteCount = await _fileService.MarkAsDeletedAsync(userId.Value, id);
        if (deleteCount == 0)
            return NotFound("File does not exist or insufficient rights");
        return Ok();
    }


}
