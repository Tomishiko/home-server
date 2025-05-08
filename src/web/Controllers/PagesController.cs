using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using core.Services;

namespace web.Controllers;

public class PagesController : Controller
{
    private readonly ILogger<PagesController> _logger;
    private readonly ICoreFS _coreFS;
    private readonly IUserService _userService;
    private readonly ILogService _logService;

    public PagesController(ILogger<PagesController> logger, ICoreFS coreFS, IUserService userService, ILogService logService)
    {
        _logger = logger;
        _coreFS = coreFS;
        _userService = userService;
        _logService = logService;
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
        ViewBag.Users = _userService.GetAll();
        return View();
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
    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        return View("Login");
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SignIn(){
        return View("AddUser");
    }
}
