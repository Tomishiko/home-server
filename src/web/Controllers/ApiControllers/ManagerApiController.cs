using core.Models;
using core.Models.Generic;
using core.Services;
using core.utils.extensions;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using System.Runtime.CompilerServices;

namespace web.Controllers;


[ApiController]
[Authorize(Policy = "ManagerOnly")]
[Route("api")]
public class ManagerApiController : ControllerBase
{
    private readonly ILogger<ManagerApiController> _logger;
    private readonly IUserService _userService;
    private readonly InvitesService _invitesService;


    public ManagerApiController(ILogger<ManagerApiController> logger,
                                IUserService userService,
                                InvitesService invites)
    {
        _logger = logger;
        _userService = userService;
        _invitesService = invites;
    }


    [HttpPost("user")]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> PostUser(
            [FromBody] RegisterManagerRequest userRequest)
    {
        var currentUserName = User.FindFirstValue(AppClaimTypes.Name);

        if (currentUserName.IsNullOrEmpty())
        {
            return Unauthorized("User identity could not be determined");
        }


        var result = await _userService.AddUserAsync(new UserCreationDto(userRequest.Username,
                                                                         userRequest.Password,
                                                                         currentUserName,
                                                                         userRequest.Role,
                                                                         userRequest.Email));
        return result switch
        {
            Success<UserDto> res => CreatedAtAction(
                                    nameof(GetUser),
                                    new { id = res.Value.Id },
                                    res.Value),

            Failure<UserDto> f => Conflict(f.Error),

            _ => StatusCode(500)

        };


    }

    [HttpDelete("user/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUser(long id)
    {


        var currentUserName = User.FindFirst(AppClaimTypes.Name)?.Value;
        if (currentUserName.IsNullOrEmpty())
        {
            return Unauthorized("User identity could not be determined");
        }

        _logger.LogInformation("Request to delete a user with id={id} uname= {uname}", id, currentUserName);


        await _userService.RemoveUserById(id, currentUserName);


        return NoContent();
    }

    [HttpGet("user/{id}")]
    [ProducesResponseType(404)]
    [ProducesResponseType(200)]
    public async Task<ActionResult<UserDto>> GetUser(long id)
    {
        UserDto? userData = await _userService.GetUserInfo(id);

        if (userData is null)
        {
            return NotFound();
        }

        return userData;
    }

    [HttpGet("users")]
    [ProducesResponseType(404)]
    [ProducesResponseType(200)]
    public async IAsyncEnumerable<UserDto> GetUsers(CancellationToken ct)
    {
        IAsyncEnumerable<UserDto> userData = _userService.GetAllUsersJoinedAsync(ct);

        if (userData is null)
        {
            yield break;
        }

        await foreach (var user in userData)
        {
            yield return user;
        }

    }
    [HttpGet("geninvite")]
    public async Task<ActionResult<InviteTokenModel>> GetNewInviteToken()
    {
        Debug.Assert(User.Identity?.Name is not null);
        InviteTokenModel token = await _invitesService.GenNewInviteAsync(User.Identity.Name);
        string encoded = WebEncoders.Base64UrlEncode(token.Value);

        return Ok(new { token = encoded, expires_at = token.Expiration });
    }
    [HttpGet("init-xsrf")]
    [AllowAnonymous]
    public IActionResult InitXsrf([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(tokens.RequestToken);
    }
    [HttpGet("logs")]
    public IAsyncEnumerable<LogDto> GetLogs([FromServices] ILogService logs, CancellationToken ct)
    {
        var timezone = HttpContext.Request.Cookies["utcOffset"];
        if (timezone.IsNullOrEmpty())
            timezone = "UTC";
        return logs.GetAll(timezone, ct);
    }
}
