using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using core.Services;
using core.utils.extensions;
using core.Models;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using web.ViewModels;
using web.Models;
using core.Models.Generic;

namespace web.Controllers;


[Authorize(Policy = "ManagerOnly")]
public class UsersManagerController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersManagerController> _logger;
    private readonly bool _isHtmx;

    public UsersManagerController(IUserService userService,
                                  ILogger<UsersManagerController> logger,
                                  IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _logger = logger;
        _isHtmx = httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("HX-Request") ?? false;
    }

    [HttpGet]
    public IActionResult Index(CancellationToken ct = default)
    {
        IAsyncEnumerable<UserDto> initialVal = _userService.GetAllUsersJoinedAsync(ct);

        return _isHtmx ? PartialView("Index", initialVal) : View("Index", initialVal);
    }
    // Partial user tablesd
    [HttpGet]
    public IActionResult PartialUsersTable(CancellationToken ct = default)
    {
        return _isHtmx
            ? PartialView("_ManageUsers", _userService.GetAllUsersJoinedAsync(ct))
            : Index(ct);
    }

    [HttpGet]
    public IActionResult AddUserPage()
    {
        ViewData["isInvite"] = false;
        return _isHtmx
            ? PartialView("NewUserPage")
            : View("NewUserPage");
    }
    [HttpPost]
    public async Task<IActionResult> AddUser([FromForm] RegisterManagerRequest userRequest,
                                             CancellationToken ct = default)
    {
        var currentUserName = User.FindFirstValue(AppClaimTypes.Name);

        if (currentUserName.IsNullOrEmpty())
        {
            return Unauthorized("User identity could not be determined");
        }
        if (!ModelState.IsValid)
        {
            return PartialView("_NewUserManagerForm", userRequest);
        }


        var result = await _userService.AddUserAsync(new UserCreationDto(userRequest.Username,
                                                                         userRequest.Password,
                                                                         currentUserName,
                                                                         userRequest.Role,
                                                                         userRequest.Email));
        return result switch
        {
            Success<UserDto> => Index(ct),

            Failure<UserDto> f => Conflict(f.Error.Message),

            _ => StatusCode(500)

        };
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromRoute] long id,
                                                CancellationToken ct = default)
    {

        var currentUserName = User.FindFirst(AppClaimTypes.Name)?.Value;
        if (currentUserName.IsNullOrEmpty())
        {
            return Unauthorized("User identity could not be determined");
        }

        _logger.LogInformation("Request to delete a user with id={id} uname= {uname}", id, currentUserName);


        await _userService.RemoveUserById(id, currentUserName);


        return _isHtmx ? PartialView("_ManageUsers", _userService.GetAllUsersJoinedAsync(ct))
            : Index(ct);
    }
}
