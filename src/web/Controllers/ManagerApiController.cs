using core.Models;
using core.Services;
using System.Diagnostics;
using core.utils.extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace web.Controllers;


[ApiController]
[Authorize(Roles = "manager")]
[Route("api")]
public class ManagerApiController : ControllerBase
{
    private ILogger<ManagerApiController> _logger;
    private IUserService _userService;
    private ILogService _logService;
    private readonly InvitesService _invites;


    public ManagerApiController(ILogger<ManagerApiController> logger,
                                IUserService userService,
                                ILogService logService,
                                InvitesService invites)
    {
        _logger = logger;
        _userService = userService;
        _logService = logService;
        _invites = invites;
    }
    [HttpPut("user")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        if (user.Uname.IsNullOrEmpty() || user.Password.IsNullOrEmpty() ||
                user.Role.IsNullOrEmpty())
            return BadRequest("Some requiered fields are empty");
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

                    Debug.Assert(User.Identity?.Name is not null);
                    Log log = new Log(0, $"Added new user {user}", DateTime.Now.ToUniversalTime(), User.Identity.Name);
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
    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUser(uint id, [FromQuery] string uname)
    {
        //_logger.LogInformation($"parameter : {user.Uname}  {user.Id}");
        if (uname.IsNullOrEmpty())
        {
            return BadRequest();
        }

        try
        {
            await _userService.RemoveUserById(id);
            _logger.LogInformation($"Deleting user with id={id} uname= {uname}");
            Debug.Assert(User.Identity is not null);
            Log log = new(0, $"Deleted user: {id}.{uname}", DateTime.Now.ToUniversalTime(), User.Identity.Name);
            await _logService.NewLogAsync(log);
            int result = await _userService.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting user");
            return base.Problem(ex.Message);
        }


        // This is temporary, fix this
        return Ok();
    }
    [HttpGet("geninvite")]
    public async Task<IActionResult> GetNewInviteToken()
    {
        Debug.Assert(User.Identity?.Name is not null);
        byte[] token = await _invites.GenNewInvite(User.Identity.Name);

        return Ok(WebEncoders.Base64UrlEncode(token));
    }
}
