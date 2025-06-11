namespace web.Controllers;

using core.Models;
using core.Services;
using System.Diagnostics;
using core.utils.extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/manager")]
[Authorize]
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

    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        if (user.Uname.IsNullOrEmpty() || user.Password.IsNullOrEmpty() ||
                user.Role.IsNullOrEmpty())
            return BadRequest("Some requiered fields are empty");
        _logger.LogInformation($"Add user request {user.Uname}");
        try
        {

            await _userService.NewUserAsync(user);
            // Log action
            Debug.Assert(User.Identity is not null);
            Log log = new Log($"Added new user {user}",DateTime.Now, User.Identity.Name);
            _logService.NewLog(log);

            // Logs and user services work with same datacontext
            await _userService.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error whle adding user to db");
        }

        return Ok();
    }
    public async Task<IActionResult> DeleteUser(uint id)
    {
        //_logger.LogInformation($"parameter : {user.Uname}  {user.Id}");

        string deleted = await _userService.RemoveUser(id);
        _logger.LogInformation($"Result of deleting user with id={id}:{deleted}");
        Debug.Assert(User.Identity is not null);
        Log log = new($"Deleted user: {deleted}",DateTime.Now,User.Identity.Name);
        _logService.NewLog(log);
        int result = await _userService.SaveChangesAsync();

        // This is temporary, fix this crapy stuff
        return result == 1 ? Ok() : BadRequest("Non existent user");
    }
}
