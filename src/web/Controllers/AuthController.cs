namespace web.Controllers;

using Microsoft.AspNetCore.Mvc;
using core.Services;
using System.Diagnostics;
using core.Models;

[ApiController]
public class AuthenticationController : Controller
{
    ILogger<AuthenticationController> _logger;
    IConfiguration _config;
    JWTGen _tokenGen;
    IAuthService _authService;

    public AuthenticationController(ILogger<AuthenticationController> logger,
        IConfiguration config, JWTGen tokenGen, IAuthService authService)
    {
        _logger = logger;
        _config = config;
        _tokenGen = tokenGen;
        _authService = authService;
    }

    [HttpPost("auth")]
    async public Task<IActionResult> Authentication([FromForm] User creds)
    {
        _logger.LogInformation($"{creds}");
        try
        {
            if (!_authService.Authenticate(ref creds))
                return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Info: Wasnt able to authenticate through DB");
            return base.Problem("DB exception");
        }
        Debug.Assert(creds.Role != null, "This field should be set in auth if succesfull");
        string token = _tokenGen.GenerateNewToken(creds.Uname, creds.Role);
        //_logger.LogInformation($"{HttpContext.Request.Headers.Accept}");

        // If request was sent from the code - return standart JWT token resoinse
        if (HttpContext.Request.Headers.Accept.Any(x => x.Contains("application/json")))
            return Ok(new
            {
                access_token = token,
                expires_in = _config["JWT:expiration"],
                type = "Bearer"
            });
        // If request is from the  browser - set cookie and redirect
        else
        {
            HttpContext.Response.Cookies.Append("AspNet.Id", token, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(double.Parse(_config["JWT:expiration"])),
                SameSite = SameSiteMode.Strict,
                HttpOnly = true
            });

            if (Request.Cookies.TryGetValue("returnUrl", out string returnUrl))
            {
                Response.Cookies.Delete("returnUrl");
                return Redirect(returnUrl);
            }
            else
                return Redirect("/");
        }

    }


}
