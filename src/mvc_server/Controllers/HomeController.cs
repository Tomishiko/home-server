using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using mvc_server.Models;
using mvc_server.Models;
using mvc_server.Services;
using System.Security.Claims;

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
        //User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "manager")
        return View(_coreFS.GetMovies);
    }
    [HttpPost("/partial")]
    [Authorize]
    public IActionResult PartialTableLoad([FromBody] PartialTableModel body) //TODO: make it index based; make separate controller for partials
    {

        //TODO: add folder string verification
        var fs = _coreFS as CoreFS;
        string? newFolder;
        var currDir = fs.GetElements(body.folder);
        newFolder = (body.id == -1) ? body.folder.Remove(body.folder.LastIndexOf('/')) :
                                $"{body.folder}/{currDir[body.id].Name}";
        ViewData["Breadcrumbs"] = newFolder;
        return PartialView("/Views/Partials/_IndexTable.cshtml", _coreFS.GetElements(newFolder));
    }
    [Authorize]
    public IActionResult Manage()
    {
        return View();
    }

    public IActionResult TvPage()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
