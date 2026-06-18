using Microsoft.AspNetCore.Mvc;
using web.Models;
using core.Services;
using Microsoft.AspNetCore.WebUtilities;
using core.Models;
using System.Diagnostics;
using core.Models.Generic;
using Microsoft.AspNetCore.Authorization;
using QRCoder;
using web.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Namotion.Reflection;
using System.Security.Claims;
using core.Domain;

namespace web.Controllers;

[Route("invite")]
public class InviteLinkController : Controller
{
    private readonly IInvitesService _invites;
    private readonly ILogger<InviteLinkController> _logger;
    private readonly IUserService _userService;
    private readonly ILogService _logService;

    public InviteLinkController(IInvitesService invites,
                                ILogger<InviteLinkController> logger,
                                IUserService userService,
                                ILogService logService)
    {
        _invites = invites;
        _logger = logger;
        _userService = userService;
        _logService = logService;
    }

    [HttpGet("geninvite")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<InviteTokenModel>> GetNewInviteToken()
    {

        if (!long.TryParse(User.FindFirstValue(AppClaimTypes.Identity),
                           out long userId))
        {
            return BadRequest("Bad session metadata");
        }

        InviteTokenModel token = await _invites.GenNewInviteAsync(userId);
        string encoded = WebEncoders.Base64UrlEncode(token.Value);

        return Ok(new NewInviteTokenResponse(encoded, token.Expiration));
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetRegisterFormInvite(string token)
    {

        var validationResult = await _invites.ValidateToken(token);

        if (validationResult is not Success<ValidTokenDetail>)
        {
            return BadRequest("Bad token data");
        }


        ViewData["isInvite"] = true;
        return View("~/Views/UsersManager/NewUserPage.cshtml");
    }

    [HttpPost("{token}")]
    public async Task<IActionResult> NewUserRegisterFromInvite(
            [FromForm] RegisterFromInviteRequest request,
            string token)
    {

        if (!ModelState.IsValid)
        {
            return PartialView("/Views/Partials/_InviteRegisterForm.cshtml", request);
        }

        var userCreation = new UserCreationDto(request.Username,
                                               request.Password,
                                               string.Empty,
                                               (byte)RoleIds.User,
                                               request.Email);


        Result<int> result = await _invites.ConsumeToken(token, userCreation);

        switch (result)
        {
            case Success<int>:
                return PartialView("/Views/Partials/_RegistrationConfirm.cshtml");

            case Failure<int> f:
                ModelState.AddModelError("Username", f.Error.Message);
                return PartialView("/Views/Partials/_InviteRegisterForm.cshtml");

            default:
                return StatusCode(500);
        }
    }
}
