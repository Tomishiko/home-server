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
    JWTGen _tokenGen;
    IAuthService _authService;
    IOptions<JWT> _options;

    public AuthenticationApiController(ILogger<AuthenticationApiController> logger,
         JWTGen tokenGen, IAuthService authService, IOptions<JWT> options)
    {
        _logger = logger;
        _tokenGen = tokenGen;
        _authService = authService;
        _options = options;
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
