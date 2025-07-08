namespace web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using core.Services;
using core.Models;

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
    public IActionResult ManageUsers()
    {
        return PartialView("/Views/Partials/_ManageUsers.cshtml", _userService.GetAllJoined());
    }
    // Partial log table
    public IActionResult ManageLogs()
    {

        return PartialView("/Views/Partials/_ManageLogs.cshtml", _logService.GetPage(0, 10));
    }

    public IActionResult LogsPartialTable([FromQuery]uint lastItem)
    {

        return PartialView("/Views/Partials/_LogsTableBody.cshtml", _logService.GetPage(lastItem, 10));
    }

    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        IEnumerable<User> initialVal = _userService.GetAllJoined();

        if (requestWith == "XMLHttpRequest")
            return PartialView(initialVal);
        else
            return View(initialVal);
    }
    [HttpGet]
    public IActionResult AddUser()
    {
        return View("AddUser");
    }
}
