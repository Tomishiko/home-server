using System.Security.Claims;
using core.Models;
using core.Models.Generic;
using core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using web.Models.RequestModels;
namespace web.Controllers;

public class AuthTestController : Controller
{
    ILogger<AuthTestController> _logger;
    IAuthService _authService;

    public AuthTestController(ILogger<AuthTestController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    async public Task<IActionResult> Authentication([FromForm] AuthRequest creds)
    {

        if(!ModelState.IsValid)
            return PartialView("Views/Login/Index.cshtml",creds);

        Result<UserDto> result = await _authService.AuthenticateAsync(
                new UserAuthDto(creds.Username, creds.Password));

        return result switch
        {
            Success<UserDto> success => await SignInHandle(success.Value),
            Failure<UserDto> failuer => Unauthorized(failuer.Error),
            _ => BadRequest()
        };

    }

    private async Task<IActionResult> SignInHandle(UserDto user)
    {

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Id", user.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return Redirect("/");

    }

}
