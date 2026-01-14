using core.Models;
using core.Services;
using System.Diagnostics;
using core.utils.extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using web.Models;
using System.Security.Claims;

namespace web.Controllers;


[ApiController]
[Authorize(Roles = "manager")]
[Route("api")]
public class ManagerApiController : ControllerBase
{
    private ILogger<ManagerApiController> _logger;
    private IUserService _userService;
    private readonly InvitesService _invites;


    public ManagerApiController(ILogger<ManagerApiController> logger,
                                IUserService userService,
                                InvitesService invites)
    {
        _logger = logger;
        _userService = userService;
        _invites = invites;
    }
    [HttpPost("user")]
    public async Task<IActionResult> AddUser([FromBody] RegisterManagerRequest userRequest)
    {
        var currentUserName = User.Identity?.Name;
        if (currentUserName.IsNullOrEmpty())
        {
            return Unauthorized("User identity could not be determined");
        }


        Result<User> result = await _userService.AddUserAsync(userRequest.Username,
                                                              userRequest.Password,
                                                              currentUserName!,
                                                              userRequest.Email,
                                                              userRequest.Role);

        if (result.status == ResultStatus.Fail)
        {
            return Conflict(result.resultMsg);
        }

        Debug.Assert(result.resultObject is not null);
        return CreatedAtAction(nameof(GetUser),
                               new { id = result.resultObject?.Id },
                               result.resultObject);
    }
    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUser(uint id)
    {


        var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (currentUserName.IsNullOrEmpty())
        {
            return Unauthorized("User identity could not be determined");
        }

        _logger.LogInformation("Request to delete a user with id={id} uname= {uname}", id, currentUserName);


        await _userService.RemoveUserById(id, currentUserName);


        return NoContent();
    }
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(uint id)
    {
        User? userData = await _userService.GetUserInfo(id);

        if (userData is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            Id = userData.Id,
            Username = userData.Uname,
            Role = userData.Role,
            Email = userData.Email
        });
    }
    [HttpGet("geninvite")]
    public async Task<IActionResult> GetNewInviteToken()
    {
        Debug.Assert(User.Identity?.Name is not null);
        byte[] token = await _invites.GenNewInvite(User.Identity.Name);

        return Ok(WebEncoders.Base64UrlEncode(token));
    }
}
