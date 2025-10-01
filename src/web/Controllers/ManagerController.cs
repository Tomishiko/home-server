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
            ? PartialView("/Views/Partials/_ManageUsers.cshtml", _userService.GetAllJoined())
            : Index(string.Empty);
    }
    // Partial log table
    public IActionResult ManageLogs([FromHeader(Name = "X-Requested-With")] string requestWith)
    {

        Console.WriteLine("hello from controller");
        return Utility.IsXmlHttpRequest(requestWith)
            ? PartialView("/Views/Partials/_ManageLogs.cshtml", _logService.GetPage(0, 10))
            : Index(string.Empty);// return Manager index page fallback
    }

    public IActionResult LogsPartialTable([FromQuery] uint lastItem)
    {

        return PartialView("/Views/Partials/_LogsTableBody.cshtml", _logService.GetPage(lastItem, 10));
    }
    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        IEnumerable<User> initialVal = _userService.GetAllJoined();
        return Utility.IsXmlHttpRequest(requestWith) ? PartialView(initialVal) : View("Index", initialVal);
    }
    [HttpPut]
    public IActionResult AddUser([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        return Utility.IsXmlHttpRequest(requestWith) ? PartialView("AddUser") : View("AddUser");
    }
}
