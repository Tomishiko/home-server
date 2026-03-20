using Microsoft.AspNetCore.Mvc;
using core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using web.Helpers;
using Microsoft.AspNetCore.Authorization;
using core.Services;

[Authorize]
public class ProfileController : Controller
{

    readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index([FromHeader(Name = "HX-Request")] bool isHtmx)
    {
        long? userId = Utility.TryGetUserId(User);
        if (userId is null) return BadRequest();

        UserDto? user = await _userService.GetUserInfo(userId.Value);
        return isHtmx ? PartialView(user) : View(user);
    }
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
