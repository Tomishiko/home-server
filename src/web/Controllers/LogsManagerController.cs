using System.Collections.Immutable;
using System.Text;
using core.Models;
using core.Services;
using core.utils.extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using web.ViewModels;

namespace web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public class LogsManagerController : Controller
{
    private readonly ILogger<LogsManagerController> _logger;
    private readonly ILogService _logService;

    public LogsManagerController(ILogger<LogsManagerController> logger, ILogService logService)
    {
        _logger = logger;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> PartialLogsTable([FromQuery] string pagination)
    {
        if (pagination.IsNullOrEmpty())
        {
            return BadRequest();
        }
        var decoded = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(pagination));
        var parts = decoded.Split('|');

        if (!long.TryParse(parts[0], out long cursorId) ||
                !DateTimeOffset.TryParse(parts[1], out DateTimeOffset eventTime))
        {
            return BadRequest("Wrong pagination format");
        }
        string? timeZone = Request.Cookies["utcOffset"];

        ImmutableArray<LogDto> data = await _logService.GetPage(10, timeZone, cursorId, eventTime);

        var vm = new LogsPartialTableViewModel
        {
            Logs = data,
            BtnDisabled = data.Length < 10,
            HideLoadMoreBtn = false,
        };


        if (data.Length == 10)
        {
            LogDto last = data[data.Length - 1];
            var cursor = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes($"{last.Id}|{last.Time:O}"));

            vm.Cursor = cursor;

        }

        return PartialView(
                "_LogsTableBody",
                vm);
    }

    [HttpGet]
    public async Task<IActionResult> PartialLogsPage([FromHeader(Name = "HX-Request")] bool isHtmx)
    {
        if (isHtmx)
        {
            string? timeZone = Request.Cookies["utcOffset"];
            ImmutableArray<LogDto> data = await _logService.GetPage(10, timeZone);
            var vm = new LogsPartialTableViewModel
            {
                Logs = data,
                BtnDisabled = data.Length < 10,
                HideLoadMoreBtn = true,
            };

            if (data.Length == 10)
            {
                LogDto last = data[data.Length - 1];
                var cursor = WebEncoders.Base64UrlEncode(
                        Encoding.UTF8.GetBytes($"{last.Id}|{last.Time:O}"));

                vm.Cursor = cursor;
            }

            return PartialView("_ManageLogs", vm);

        }
        else return RedirectToAction(nameof(UsersManagerController.Index), "Manager");
    }
}
