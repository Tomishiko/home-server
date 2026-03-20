using System.Security.Claims;
using System.Text.Json;
using core.Models;
using core.Models.Generic;
using core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using web.Helpers;
using web.Models.RequestModels;

namespace web.Controllers;

public class LoginController : Controller
{

    public IActionResult Index([FromHeader(Name = "HX-Request")] bool isHtmx)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            HttpContext.Response.Headers["HX-Redirect"] = "/";
            return NoContent();
        }
        
        return isHtmx ? PartialView() : View();
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromForm] AuthRequest request,
                                           [FromQuery] string? ReturnUrl,
                                           [FromServices] IAuthService authService)
    {
        if (!ModelState.IsValid)
        {
            return PartialView(request);
        }
        Result<UserDto> result = await authService.AuthenticateAsync(
                new UserAuthDto(request.Username, request.Password));

        switch (result)
        {
            case Success<UserDto> success:
                var claimsIdentity = Utility.BuildClaims(success.Value);

                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                if (string.IsNullOrEmpty(ReturnUrl))
                    ReturnUrl = "/";
                HttpContext.Response.Headers["HX-Redirect"] = ReturnUrl;
                return NoContent();


            case Failure<UserDto> failuer:
                ViewData["ErrorMessage"] = failuer.Error.Message;
                ViewData["ReturnUrl"] = ReturnUrl;
                return PartialView(request);

            default: return StatusCode(500);
        }
    }
}
