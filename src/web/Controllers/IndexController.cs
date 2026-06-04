using Microsoft.AspNetCore.Mvc;
using core.Services;
using web.Models;
using System.Security.Claims;
using web.ViewModels;
using web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace web.Controllers;


public class IndexController : Controller
{
    private readonly ILogger<IndexController> _logger;
    private readonly IFileService _fileService;

    public IndexController(ILogger<IndexController> logger,
                           IFileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    [HttpGet]
    public IActionResult Index([FromHeader(Name = "HX-Request")] bool isHtmx, CancellationToken ct)
    {
        var fileInfo = _fileService.GetSharedFilesAsync(ct);

        if (isHtmx)
            return PartialView(new IndexPageViewModel
            {
                Files = fileInfo,
                IsPrivate = false
            });
        else
            return View(new IndexPageViewModel
            {
                Files = fileInfo,
                IsPrivate = false
            });
    }
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteFile([FromQuery] long id,
                                    [FromServices] IFileService fileService,
                                    CancellationToken ct)
    {

        long? userId = Utility.TryGetUserId(User);

        if (userId is null)
        {
            return Forbid("Unable to identify user");
        }

        int deleteCount = await fileService.MarkAsDeletedAsync(userId.Value, id);
        if (deleteCount == 0)
            return NotFound("File does not exist or insufficient rights");

        return PartialTableLoad(NavigationAction.Public, ct);
    }

    [HttpGet]
    public IActionResult PartialTableLoad([FromQuery] NavigationAction action = NavigationAction.Public, CancellationToken ct = default)
    {
        var uid = User.FindFirstValue("Id");
        if (action == NavigationAction.Private && uid == null)
        {
            HttpContext.Response.Headers["HX-Redirect"] = $"/login";
            return Unauthorized();
        }

        return action switch
        {
            NavigationAction.Private => PartialView("_IndexTable",
                    new IndexPageViewModel
                    {
                        Files = _fileService.GetPrivateFilesAsync(long.Parse(uid!), ct),
                        IsPrivate = true
                    }),

            NavigationAction.Public => PartialView("_IndexTable",
                    new IndexPageViewModel
                    {
                        Files = _fileService.GetSharedFilesAsync(ct),
                        IsPrivate = false,
                    }),

            _ => BadRequest()

        };
    }
}
