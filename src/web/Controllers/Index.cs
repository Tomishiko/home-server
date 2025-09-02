namespace web.Controllers;

using core.Services;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Security.Claims;

public class IndexController : Controller
{
    private readonly ILogger<IndexController> _logger;
    private readonly ICoreFS _coreFS;
    private readonly IUserService _userService;
    private readonly ILogService _logService;
    private readonly IFileService _fileService;

    public IndexController(ILogger<IndexController> logger, ICoreFS coreFS, IUserService userService, ILogService logService, IFileService fileService)
    {
        _logger = logger;
        _coreFS = coreFS;
        _userService = userService;
        _logService = logService;
        _fileService = fileService;
    }

    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        ViewData["Breadcrumbs"] = "root/";
        //IEnumerable<FileInfo> fileInfo = _coreFS.GetIndexFiles;
        var fileInfo = _fileService.GetSharedFilesAsync();
        if (requestWith == "XMLHttpRequest")
            return PartialView(fileInfo);
        else
            return View(fileInfo);
    }
    [Authorize]
    [HttpPost("/partialtable")]
    public IActionResult PartialTableLoad([FromBody] PartialTableModel body)
    {
        Claim? cuId = User.FindFirst("Id");
        Debug.Assert(cuId is not null);
        string subroute = body.action == NavigationAction.Public ? "" : body.action.ToString();
        ViewData["Breadcrumbs"] = $"root/{subroute}";
        return body.action switch
        {
            NavigationAction.Private => PartialView("/Views/Partials/_IndexTable.cshtml",
                    _fileService.GetPrivateFilesAsync(uint.Parse(cuId.Value))),

            NavigationAction.Public => PartialView("/Views/Partials/_IndexTable.cshtml",
                    _fileService.GetSharedFilesAsync()),

            _ => BadRequest()

        };
    }
}
