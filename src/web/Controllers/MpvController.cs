using Microsoft.AspNetCore.Mvc;
using core.Services;
using System.Diagnostics;
namespace web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MpvController : ControllerBase
{

    private ILogger<MpvController> _logger;
    private IMpvService _mpvService;
    private readonly string[] _allowedCommands = new string[]{
        "cycle pause",
        "stop",
        "seek +10",
        "seek -10"
    };

    public MpvController(ILogger<MpvController> logger, IMpvService mpvService)
    {
        _logger = logger;
        _mpvService = mpvService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartMpv()
    {

        string? url;
        using (var reader = new StreamReader(Request.Body))
        {
            url = await reader.ReadLineAsync();
        }

        if (string.IsNullOrEmpty(url))
            return BadRequest();

        try
        {
            _logger.LogInformation($"Starting mpv: {url}");
            url = await _mpvService.StartMpv(url);
            _logger.LogInformation("Succesfully exited mpv");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception when trying to start MPV {ex}", ex);
        }


        return Ok();
    }
    [HttpGet("command")]
    public async Task<IActionResult> MpvCommand([FromQuery] string cmd)
    {

        try
        {
            using Process? socat = Process.Start(new ProcessStartInfo
            {
                FileName = "socat",
                Arguments = "- /tmp/mpvsocket",
                RedirectStandardInput = true,
            });
            if (!_allowedCommands.Contains(cmd))
            {
                return Forbid();

            }
            await socat.StandardInput.WriteLineAsync(cmd);
            _logger.LogInformation($"PERFORMING {cmd}");

        }
        catch (Exception ex)
        {
            _logger.LogError("Problem with socat", ex);

            return Problem();
        }

        return Ok();

    }
}
