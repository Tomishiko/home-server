namespace web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using core.Services;
using core.utils.extensions;
using core.Models;
using web.Helpers;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Immutable;

[Authorize(Policy = "ManagerOnly")]
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
    public async Task<IActionResult> ManageLogs([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        string? timeZone = Request.Cookies["utcOffset"];
        ImmutableArray<LogDto> data = await _logService.GetPage(10, timeZone);
        if (data.Length == 10)
        {
            LogDto last = data[data.Length - 1];
            var cursor = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes($"{last.Id}|{last.Time:O}"));

            ViewData["Cursor"] = cursor;
        }

        return Utility.IsXmlHttpRequest(requestWith)
            ? PartialView("/Views/Partials/_ManageLogs.cshtml", data)
            : Index(string.Empty);// return Manager index page fallback
    }

    public async Task<IActionResult> LogsPartialTable([FromQuery] string pagination)
    {
        if (pagination.IsNullOrEmpty())
        {
            return BadRequest();
        }
        var decoded = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(pagination));
        var parts = decoded.Split('|');

        long cursorId;
        DateTimeOffset eventTime;

        _logger.LogCritical(parts[1]);

        if (!long.TryParse(parts[0], out cursorId) ||
                !DateTimeOffset.TryParse(parts[1], out eventTime))
        {
            return BadRequest("Wrong pagination format");
        }
        string? timeZone = Request.Cookies["utcOffset"];

        ImmutableArray<LogDto> data = await _logService.GetPage(10, timeZone, cursorId, eventTime);

        if (data.Length == 10)
        {
            LogDto last = data[data.Length - 1];
            var cursor = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes($"{last.Id}|{last.Time:O}"));

            Response.Headers.Add("X-Next-Cursor", cursor);

        }
        return PartialView(
                "/Views/Partials/_LogsTableBody.cshtml",
                data);
    }

    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        IEnumerable<UserDto> initialVal = _userService.GetAllUsersJoined();
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
