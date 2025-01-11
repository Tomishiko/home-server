using mvc_server.Controllers;
using Microsoft.AspNetCore.Mvc;
using mvc_server.Models;

[ApiController]
public class AuthenticationController : Controller
{
    private ILogger<AuthenticationController> _logger;
    private IConfiguration _config;
    private JWTGen _tokenGen;
    public AuthenticationController(ILogger<AuthenticationController> logger, IConfiguration config, JWTGen tokenGen)
    {
        _logger = logger;
        _config = config;
        _tokenGen = tokenGen;
    }

    [HttpPost("auth")]
    public IActionResult Authentication([FromForm] AuthModel auth)
    {
        if (Request.ContentType != "application/x-www-form-urlencoded")
            return BadRequest(new { Error = "Invalid request" });

        if (auth is { Name: "admin", Password: "admin" })//TODO add checking from DB
        {
            string token = _tokenGen.GenerateNewToken(auth.Name);
            return Ok(new{
                        access_token = token,
                        expires_in = _config["JWT:expiration"],
                        type = "Bearer"
                    });
        }
        return Unauthorized();
    }
    [HttpGet("login")]
    public async Task<IActionResult> Login(){
        return View("Login");
    }


}
