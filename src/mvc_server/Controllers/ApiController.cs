using Microsoft.AspNetCore.Mvc;
using mvc_server.Models;
using mvc_server.Services;
using System.Web;

namespace mvc_server.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ICoreFS coreFS;
    private readonly ILogger<ApiController> _logger;

    public ApiController(ICoreFS coreFS, ILogger<ApiController> logger)
    {
        this.coreFS = coreFS;
        this._logger = logger;
    }
    [HttpGet("video/{id}")]
    public IActionResult GetVideo(int id)
    {
        var videos = coreFS.GetMovies;
        var file = videos.ToArray()[id];
        var fs = file.OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: file.Name);
    }
    [HttpGet("file/{id}")]
    public IActionResult GetFile(int id)
    {
        var files = coreFS.GetIndexFiles;
        var file = files.ToArray()[id];

        var fs = file.OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: file.Name);
    }
    [HttpPost("auth")]
    public IActionResult Authentication([FromBody]AuthModel auth){
        if(auth is {Name:"TestName",Password:"TestPwd"}){
            return Ok(new{AccesToken = "token"});
        }
        return Unauthorized();
    }


}
