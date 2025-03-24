using System.Diagnostics;
using core.Models;
using Data.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using web.Models;
using web.Models;
using web.Services;
using System.Security.Claims;

namespace web.Controllers;

public class PagesController : Controller
{
    private readonly ILogger<PagesController> _logger;
    private readonly ICoreFS _coreFS;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Log> _logsRepo;

    public PagesController(ILogger<PagesController> logger, ICoreFS coreFS,IRepository<User> userRepo, IRepository<Log> logsRepo)
    {
        _logger = logger;
        _coreFS = coreFS;
        this._userRepo = userRepo;
        this._logsRepo = logsRepo;
    }
    public IActionResult Index()
    {
        ViewData["Breadcrumbs"] = "wwwroot/files";
        return View(_coreFS.GetIndexFiles);
    }
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
        ViewBag.Users = _userRepo.GetAll();
        return View();
    }
    [Authorize]
    public IActionResult ManageUsers(){
        return PartialView("/Views/Partials/_ManageUsers.cshtml",_userRepo.GetAll());
    }
    [Authorize]
    public IActionResult ManageLogs(){
        return PartialView("/Views/Partials/_ManageLogs.cshtml",_logsRepo.GetAll());
    }

    public IActionResult Tv()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
