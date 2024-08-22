using Microsoft.AspNetCore.Mvc;
using mvc_server.Services;

namespace mvc_server.Controllers;

public class ApiController : Controller
{
    private readonly ICoreFS coreFS;

    public ApiController(ICoreFS coreFS)
    {
        this.coreFS = coreFS;
    }
    [Route("api/video/{id}")]
    public IActionResult GetVideo(int id)
    {
        var videos = coreFS.GetMovies;
        var fs = videos[id].OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: videos[id].Name);
    }
    [Route("api/file/{id}")]
    public IActionResult GetFile(int id)
    {
        var files = coreFS.GetFiles;
        var fs = files[id].OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: files[id].Name);
    }
}