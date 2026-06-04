namespace web.Controllers;

using Microsoft.AspNetCore.Mvc;
using core.Services;
using core.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using core.Models.Generic;
using web.Models.RequestModels;
using Microsoft.AspNetCore.Antiforgery;
using web.Helpers;

[ApiController]
public class AuthenticationApiController : ControllerBase
{
    ILogger<AuthenticationApiController> _logger;
    IAuthService _authService;

    public AuthenticationApiController(ILogger<AuthenticationApiController> logger,
            IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpPost("auth")]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    async public Task<IActionResult> Authentication([FromForm] AuthRequest creds)
    {

        Result<UserDto> result = await _authService.AuthenticateAsync(
                new UserAuthDto(creds.Username, creds.Password));

        switch (result)
        {
            case Success<UserDto> success:
                var claimsIdentity = Utility.BuildClaims(success.Value);

                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                return Ok();


            case Failure<UserDto> failuer:
                return Unauthorized(failuer.Error);

            default: return BadRequest();
        }
    }



}
