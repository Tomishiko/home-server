namespace web.Controllers;

using core.Models;
using core.Services;
using System.Diagnostics;
using core.utils.extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize(Roles = "manager")]
[Route("api")]
public class ManagerApiController : ControllerBase
{
    private ILogger<ManagerApiController> _logger;
    private IUserService _userService;
    private ILogService _logService;


    public ManagerApiController(ILogger<ManagerApiController> logger, IUserService userService, ILogService logService)
    {
        _logger = logger;
        _userService = userService;
        _logService = logService;
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

            await _userService.AddUserAsync(user);
            // Log action
            Debug.Assert(User.Identity is not null);
            Log log = new Log($"Added new user {user}", DateTime.Now, User.Identity.Name);
            await _logService.NewLogAsync(log);

            // Logs and user services work with same datacontext
            await _userService.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding user to db");
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
            _userService.RemoveUserById(id);
            _logger.LogInformation($"Deleting user with id={id} uname= {uname}");
            Debug.Assert(User.Identity is not null);
            Log log = new($"Deleted user: {id}.{uname}", DateTime.Now.ToUniversalTime(), User.Identity.Name);
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
}
