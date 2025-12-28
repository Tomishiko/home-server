using Microsoft.AspNetCore.Mvc;
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
    private ILogService _logService;

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
        byte[] bytes = WebEncoders.Base64UrlDecode(token);

        User? issuer = await _invites.ValidateToken(bytes);
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
    public async Task<IActionResult> NewUserRegisterFromInvite([FromForm] User user)
    {

        if (user.Uname.IsNullOrEmpty() || user.Password.IsNullOrEmpty())
            return BadRequest("Some requiered fields are empty");
        if (!Request.Cookies.TryGetValue("regID", out string? invToken) || invToken.IsNullOrEmpty())
            return BadRequest("Missing token");

        User? issuer = await _invites.ValidateToken(WebEncoders.Base64UrlDecode(invToken));
        if (issuer is null)
            return BadRequest();

        _logger.LogInformation($"Add user request {user.Uname}");
        try
        {

            Result<string> result = await _userService.AddUserAsync(user);
            // Log action
            switch (result.status)
            {
                case ResultStatus.Fail: return Conflict(result.resultObject);

                case ResultStatus.Error: return Problem(result.resultObject);

                case ResultStatus.Success:
                    Log log = new Log(0, $"Added new user {user}", DateTime.Now.ToUniversalTime(), issuer.Uname);
                    await _logService.NewLogAsync(log);

                    // Logs and user services work with same datacontext
                    await _userService.SaveChangesAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding user to db");
            return base.Problem();
        }

        return Ok();

    }
}

