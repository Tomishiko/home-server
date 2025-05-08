namespace web.Controllers;
using core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using core.Services;


[Authorize(Roles="manager")]
public class ManagerController : Controller
{
    IUserService _userService;
    ILogService _logService;
    ILogger<ManagerController> _logger;

    public ManagerController(ILogger<ManagerController> logger, ILogService logService, IUserService userService)
    {
        _logger = logger;
        _logService = logService;
        _userService = userService;
    }
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        if (string.IsNullOrEmpty(user.Uname) ||
           string.IsNullOrEmpty(user.Password))
            return BadRequest("Some requiered fields are empty");
        _logger.LogInformation($"Add user request {user.Uname}");
        await _userService.NewUserAsync(user);
        await _userService.SaveChangesAsync();

        return Ok();
    }

    public async Task<IActionResult> DeleteUser(uint id)
    {
        //_logger.LogInformation($"parameter : {user.Uname}  {user.Id}");

        bool result = await _userService.RemoveUser(id);
        _logger.LogInformation($"Result of deleting user with id={id}:{result}");

        return result ? Ok() : BadRequest("Non existent user");
    }
    public IActionResult ManageUsers(){
        return PartialView("/Views/Partials/_ManageUsers.cshtml",_userService.GetAll());
    }
    public async Task<IActionResult> ManageLogs(){
        return PartialView("/Views/Partials/_ManageLogs.cshtml",_logService.GetAll());
    }
}
