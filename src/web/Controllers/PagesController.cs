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
    [Authorize]
    public IActionResult Movies()
    {
        //User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "manager")
        return View(_coreFS.GetMovies);
    }
    [HttpPost("/partial")]
    [Authorize]
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
