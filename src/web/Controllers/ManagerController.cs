namespace web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using core.Services;
using core.Models;
using web.Helpers;

[Authorize(Roles = "manager")]
public class ManagerController : Controller
{
    private readonly ILogService _logService;
    private readonly IUserService _userService;
    private readonly ILogger<ManagerController> _logger;

    public ManagerController(ILogService logService, IUserService userService, ILogger<ManagerController> logger)
    {
        this._logService = logService;
        _userService = userService;
        _logger = logger;
    }


    // Partial user tablesd
    public IActionResult ManageUsers([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        return Utility.IsXmlHttpRequest(requestWith)
            ? PartialView("/Views/Partials/_ManageUsers.cshtml", _userService.GetAllUsersJoined())
            : Index(string.Empty);
    }
    // Partial log table
    public IActionResult ManageLogs([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        string? timeZone = Request.Cookies["timeZone"];
        return Utility.IsXmlHttpRequest(requestWith)
            ? PartialView("/Views/Partials/_ManageLogs.cshtml", _logService.GetPage(0, 10, timeZone))
            : Index(string.Empty);// return Manager index page fallback
    }

    public IActionResult LogsPartialTable([FromQuery] uint lastItem)
    {
        string? timeZone = Request.Cookies["timeZone"];
        return PartialView("/Views/Partials/_LogsTableBody.cshtml", _logService.GetPage(lastItem, 10, timeZone));
    }

    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        IEnumerable<User> initialVal = _userService.GetAllUsersJoined();
        return Utility.IsXmlHttpRequest(requestWith) ? PartialView(initialVal) : View("Index", initialVal);
    }

    public IActionResult AddUser([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        ViewData["isInvite"] = false;
        return Utility.IsXmlHttpRequest(requestWith)
            ? PartialView("~/Views/Shared/NewUserPage.cshtml")
            : View("~/Views/Shared/NewUserPage.cshtml");
    }
}
