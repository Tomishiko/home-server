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
    public IResult GetVideo(int id)
    {
        var videos = coreFS.GetMovies;
        var fs = videos[id].OpenRead();
        return Results.File(fs, contentType: "video/mp4", enableRangeProcessing: true);
    }
}