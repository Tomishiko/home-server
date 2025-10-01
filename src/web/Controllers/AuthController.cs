namespace web.Controllers;

using Microsoft.AspNetCore.Mvc;
using core.Services;
using System.Diagnostics;
using core.Models;
using Microsoft.Extensions.Options;

[ApiController]
public class AuthenticationController : ControllerBase
{
    ILogger<AuthenticationController> _logger;
    JWTGen _tokenGen;
    IAuthService _authService;
    IOptions<JWT> _options;

    public AuthenticationController(ILogger<AuthenticationController> logger,
         JWTGen tokenGen, IAuthService authService, IOptions<JWT> options)
    {
        _logger = logger;
        _tokenGen = tokenGen;
        _authService = authService;
        _options = options;
    }

    [HttpPost("auth")]
    async public Task<IActionResult> Authentication([FromForm] User creds)
    {
        //_logger.LogInformation($"{creds}");
        AuthResult result;
        try
        {
            result = await _authService.AuthenticateAsync(creds);
            if (!result.isSuccesful)
                return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Info: Wasnt able to authenticate through DB");
            return Problem("DB exception");
        }
        creds = result.user;
        Debug.Assert(creds.Role != null, "This field should be set in auth if succesfull");
        string token = _tokenGen.GenerateNewToken(creds);
        //_logger.LogInformation($"{HttpContext.Request.Headers.Accept}");

        // If request was sent from the code - return standart JWT token response
        if (HttpContext.Request.Headers.Accept.Any(x => (x ??= "").Contains("application/json")))
            return Ok(new
            {
                access_token = token,
                expires_in = _options.Value.expiration,
                type = "Bearer"
            });
        // If request is from the  browser - set cookie and redirect
        else
        {
            HttpContext.Response.Cookies.Append("AspNet.Id", token, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(double.Parse(_options.Value.expiration)),
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Secure = true,
                IsEssential = true
            });

            if (Request.Cookies.TryGetValue("returnUrl", out string? returnUrl))
            {
                Response.Cookies.Delete("returnUrl");
                return Redirect(returnUrl);
            }
            else
                return Redirect("/");
        }

    }


}
