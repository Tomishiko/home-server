using Microsoft.AspNetCore.Mvc;
using core.Services;
using web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Security.Claims;

namespace web.Controllers;


public class IndexController : Controller
{
    private readonly ILogger<IndexController> _logger;
    private readonly IUserService _userService;
    private readonly ILogService _logService;
    private readonly IFileService _fileService;

    public IndexController(ILogger<IndexController> logger,
                           IUserService userService,
                           ILogService logService,
                           IFileService fileService)
    {
        _logger = logger;
        _userService = userService;
        _logService = logService;
        _fileService = fileService;
    }

    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        ViewData["Breadcrumbs"] = "root/";
        var fileInfo = _fileService.GetSharedFilesAsync();

        if (requestWith == "XMLHttpRequest")
            return PartialView(fileInfo);
        else
            return View(fileInfo);
    }
    [HttpGet("/partialtable")]
    public IActionResult PartialTableLoad([FromQuery] NavigationAction action)
    {
        var uid = User.FindFirstValue("Id");
        if (action == NavigationAction.Private && uid == null)
        {
            return Unauthorized();
        }

        return action switch
        {
            NavigationAction.Private => PartialView("/Views/Partials/_IndexTable.cshtml",
                    _fileService.GetPrivateFilesAsync(long.Parse(uid!))),

            NavigationAction.Public => PartialView("/Views/Partials/_IndexTable.cshtml",
                    _fileService.GetSharedFilesAsync()),

            _ => BadRequest()

        };
    }
}
