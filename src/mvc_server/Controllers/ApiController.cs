using Microsoft.AspNetCore.Mvc;
using mvc_server.Models;
using mvc_server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Web;
using Microsoft.AspNetCore.Authorization;

namespace mvc_server.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ICoreFS _coreFs;
    private readonly ILogger<ApiController> _logger;

    public ApiController(ICoreFS coreFS, ILogger<ApiController> logger)
    {
        this._coreFs = coreFS;
        this._logger = logger;
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


}
