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
        ViewData["Breadcrumbs"] = "wwwroot/files";
        //IEnumerable<FileInfo> fileInfo = _coreFS.GetIndexFiles;
        var fileInfo = _fileService.GetFilesAsync();
        if (requestWith == "XMLHttpRequest")
            return PartialView(fileInfo);
        else
            return View(fileInfo);
    }
    [Authorize]
    [HttpPost("/partialtable")]
    public IActionResult PartialTableLoad([FromBody] PartialTableModel body)
    {

        var fs = (CoreFS)_coreFS;
        string? newFolder;
        var currDir = fs.GetElements(body.folder);
        newFolder = (body.id == -1) ? body.folder.Remove(body.folder.LastIndexOf('/')) :
                                $"{body.folder}/{currDir[body.id].Name}";
        ViewData["Breadcrumbs"] = newFolder;
        return PartialView("/Views/Partials/_IndexTable.cshtml", _fileService.GetFilesAsync());
    }
}
