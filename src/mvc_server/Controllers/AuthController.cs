using mvc_server.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc_server.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;

[ApiController]
public class AuthenticationController : Controller
{
    private ILogger<AuthenticationController> _logger;
    private IConfiguration _config;
    private JWTGen _tokenGen;
    public AuthenticationController(ILogger<AuthenticationController> logger,
            IConfiguration config, JWTGen tokenGen)
    {
        _logger = logger;
        _config = config;
        _tokenGen = tokenGen;
    }

    [HttpPost("auth")]
    async public Task<IActionResult> Authentication([FromForm] AuthModel auth)
    {

        if (auth is not { Name: "admin", Password: "admin" })//TODO add checking from DB
            return Unauthorized();

        string token = _tokenGen.GenerateNewToken(auth.Name);
        _logger.LogInformation($"{HttpContext.Request.Headers.Accept}");
        if (HttpContext.Request.Headers.Accept.Any(x => x.Contains("application/json")))
            return Ok(new
            {
                access_token = token,
                expires_in = _config["JWT:expiration"],
                type = "Bearer"
            });
        else
        {
            HttpContext.Response.Cookies.Append("AspNet.Id", token, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(double.Parse(_config["JWT:expiration"])),
                SameSite = SameSiteMode.Strict,
                Secure = true,
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
    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        return View("Login");
    }


}
