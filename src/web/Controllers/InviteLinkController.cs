using Microsoft.AspNetCore.Mvc;
using web.Models;
using core.Services;
using core.utils.extensions;
using Microsoft.AspNetCore.WebUtilities;
using core.Models;
using System.Diagnostics;

namespace web.Controllers;

[Route("invite")]
public class InviteLinkController : Controller
{
    private readonly InvitesService _invites;
    private readonly ILogger<InviteLinkController> _logger;
    private readonly IUserService _userService;
    private readonly ILogService _logService;

    public InviteLinkController(InvitesService invites,
                                ILogger<InviteLinkController> logger,
                                IUserService userService,
                                ILogService logService)
    {
        _invites = invites;
        _logger = logger;
        _userService = userService;
        _logService = logService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetRegisterFormInvite(string token)
    {

        User? issuer = await _invites.ValidateToken(token);
        if (issuer is null)
        {
            return BadRequest("Invalid invite token");
        }

        Response.Cookies.Append("regID", token, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(5),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            Secure = true,
            IsEssential = true
        });


        ViewData["isInvite"] = true;
        return View("~/Views/Shared/NewUserPage.cshtml");
    }

    [HttpPost("register")]
    public async Task<IActionResult> NewUserRegisterFromInvite(
            [FromForm] RegisterFromInviteRequest request)
    {
        if (!Request.Cookies.TryGetValue("regID", out string? invToken)
            || invToken.IsNullOrEmpty())
        {
            return BadRequest("Missing token");

        }

        User? issuer = await _invites.ValidateToken(invToken);
        if (issuer is null)
            return BadRequest();

        Debug.Assert(issuer.Uname is not null);
        Result<User> result = await _userService.AddUserAsync(request.Username,
                                                                request.Password,
                                                                issuer.Uname,
                                                                request.Email);

        if (result.status == ResultStatus.Fail)
        {
            return Conflict(result.resultObject);
        }

        return Created();
    }
}
