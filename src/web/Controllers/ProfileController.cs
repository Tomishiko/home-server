using Microsoft.AspNetCore.Mvc;
using core.Models;
using web.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using core.Services;

[Authorize]
public class ProfileController : Controller
{

    readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {
        uint? userId = Utility.TryGetUserId(User);
        if (userId is null) return BadRequest();

        User user = await _userService.GetUserInfo(userId.Value);
        return Utility.IsXmlHttpRequest(requestWith) ? PartialView(user) : View(user);
    }
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AspNet.Id");
        return Redirect("/");
    }
}
