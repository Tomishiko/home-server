using Microsoft.AspNetCore.Mvc;
using web.Models;
using core.Services;
using core.utils.extensions;
using Microsoft.AspNetCore.WebUtilities;
using core.Models;
using System.Diagnostics;
using core.Models.Generic;

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

        UserDto? issuer = await _invites.ValidateToken(token);
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
            [FromForm] RegisterFromInviteRequest request) // TODO: shit is null for some reson
    {
        _logger.LogDebug(request.ToString());
        if (!Request.Cookies.TryGetValue("regID", out string? invToken)
            || invToken.IsNullOrEmpty())
        {
            return BadRequest("Missing token");

        }

        UserDto? issuer = await _invites.ValidateToken(invToken);
        if (issuer is null)
            return BadRequest();

        Debug.Assert(issuer.Username is not null);

        var userCreation = new UserCreationDto(request.Username,
                request.Password, issuer.Username, (byte)Roles.User, request.Email);
        Result<UserDto> result = await _userService.AddUserAsync(userCreation);
        Response.Cookies.Delete("regID");
        return result switch
        {
            Success<UserDto> s => CreatedAtAction(nameof(ManagerApiController.GetUser),
                                               new { id = s.Value.Id },
                                               s.Value),
            Failure<UserDto> f => BadRequest(f.Error),

            _ => StatusCode(500)
        };

    }
}
