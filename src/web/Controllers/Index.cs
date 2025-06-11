namespace web.Controllers;

using core.Services;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using Microsoft.AspNetCore.Authorization;

public class IndexController : Controller
{
    private readonly ILogger<IndexController> _logger;
    private readonly ICoreFS _coreFS;
    private readonly IUserService _userService;
    private readonly ILogService _logService;

    public IndexController(ILogger<IndexController> logger, ICoreFS coreFS, IUserService userService, ILogService logService)
    {
        _logger = logger;
        _coreFS = coreFS;
        _userService = userService;
        _logService = logService;
    }

    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        ViewData["Breadcrumbs"] = "wwwroot/files";
        IEnumerable<FileInfo> fileInfo = _coreFS.GetIndexFiles;
        if (requestWith == "XMLHttpRequest")
            return PartialView(fileInfo);
        else
            return View(fileInfo);
    }
    [Authorize]
    [HttpPost("/partialtable")]
    public IActionResult PartialTableLoad([FromBody] PartialTableModel body) //TODO: make it index based; make separate controller for partials
    {

        //TODO: add folder string verification
        var fs = (CoreFS)_coreFS;
        string? newFolder;
        var currDir = fs.GetElements(body.folder);
        newFolder = (body.id == -1) ? body.folder.Remove(body.folder.LastIndexOf('/')) :
                                $"{body.folder}/{currDir[body.id].Name}";
        ViewData["Breadcrumbs"] = newFolder;
        return PartialView("/Views/Partials/_IndexTable.cshtml", _coreFS.GetElements(newFolder));
    }
}
