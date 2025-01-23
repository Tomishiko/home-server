using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using mvc_server.Models;
using mvc_server.Models;
using mvc_server.Services;

namespace mvc_server.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICoreFS _coreFS;
    public HomeController(ILogger<HomeController> logger, ICoreFS coreFS)
    {
        _logger = logger;
        _coreFS = coreFS;
    }
    public IActionResult Index()
    {
        ViewData["Breadcrumbs"] = "wwwroot/files";
        return View(_coreFS.GetIndexFiles);
    }
    [Route("/movies")]
    [Authorize]
    public IActionResult Movies()
    {
        return View(_coreFS.GetMovies);
    }
    [HttpPost("/partial")]
    public IActionResult PartialTableLoad(int id, string folder) //TODO: make it index based; make separate controller for partials
    {
        //TODO: add folder string verification
        var fs = _coreFS as CoreFS;
        string? newFolder;
        var currDir = fs.GetElements(folder);
        newFolder = (id == -1) ? folder.Remove(folder.LastIndexOf('/')) :
                                $"{folder}/{currDir[id].Name}";
        ViewData["Breadcrumbs"] = newFolder;
        return PartialView("/Views/Partials/_IndexTable.cshtml", _coreFS.GetElements(newFolder));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
