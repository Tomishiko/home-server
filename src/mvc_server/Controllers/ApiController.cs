using Microsoft.AspNetCore.Mvc;
using mvc_server.Models;
using mvc_server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace mvc_server.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ICoreFS _coreFs;
    private readonly ILogger<ApiController> _logger;
    private readonly string[] _allowedCommands = new string[]{
        "cycle pause",
        "stop",
        "seek +5",
        "seek -5"
    };

    public ApiController(ICoreFS coreFS, ILogger<ApiController> logger)
    {
        this._coreFs = coreFS;
        this._logger = logger;
    }
    [HttpGet("video/{id}")]
    public IActionResult GetVideo(int id)
    {
        var videos = _coreFs.GetMovies;
        var file = videos.ToArray()[id];
        var fs = file.OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: file.Name);
    }
    [HttpGet("file/{id}")]
    [Authorize]
    public IActionResult GetFile(int id)
    {
        var files = _coreFs.GetIndexFiles;
        var file = files.ToArray()[id];

        var fs = file.OpenRead();
        return File(fs, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: file.Name);
    }
    [HttpPost("mpv/new")]
    public async Task<IActionResult> StartMpvAni()
    {

        string? url;
        using (var reader = new StreamReader(Request.Body))
        {
            url = await reader.ReadLineAsync();
        }

        if (string.IsNullOrEmpty(url))
            return BadRequest();

        using (Process mpv = new Process())
        {
            //mpv.StartInfo.UseShellExecute = true;
            mpv.StartInfo.FileName = "mpv";
            mpv.StartInfo.Arguments = $"{url} --input-ipc-server=/tmp/mpvsocket --fs";

            //mpv.StartInfo.RedirectStandardOutput = true;
            mpv.Start();
            _logger.LogInformation("Started new instance of mpv");
            _logger.LogInformation($"Movie url:{url}");
            await mpv.WaitForExitAsync();
            mpv.Kill();
            _logger.LogInformation("Killed mpv");

        }

        return Ok();
    }
    [HttpPost("mpv")]
    public async Task<IActionResult> StartMpv()
    {

        string? url;
        using (var reader = new StreamReader(Request.Body))
        {
            url = await reader.ReadLineAsync();
        }

        if (string.IsNullOrEmpty(url))
            return BadRequest();

        url = await GetLinks(url);

        using (Process mpv = new Process())
        {
            //mpv.StartInfo.UseShellExecute = true;
            mpv.StartInfo.FileName = "mpv";
            mpv.StartInfo.Arguments = $"{url} --input-ipc-server=/tmp/mpvsocket --fs";

            mpv.StartInfo.RedirectStandardOutput = true;
            mpv.Start();
            _logger.LogInformation("Started new instance of mpv");
            _logger.LogInformation($"Movie url:{url}");
            await mpv.WaitForExitAsync();
            mpv.Kill();
            _logger.LogInformation("Killed mpv");

        }

        return Ok();
    }
    [HttpGet("mpv/command")]
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
            //_logger.LogInformation($"PERFORMING {cmd}");

        }
        catch (Exception ex)
        {
            _logger.LogError("Problem with socat", ex);

            return Problem();
        }

        return Ok();

    }
    [HttpGet("mpv/pause")]
    public async Task<IActionResult> PauseMpv()
    {
        Process? socat = Process.Start(new ProcessStartInfo
        {
            FileName = "socat",
            Arguments = "- /tmp/mpvsocket",
            RedirectStandardInput = true,
        });
        _logger.LogInformation("Sending stop request to socat");
        // cycle pause
        await socat.StandardInput.WriteLineAsync("stop");
        _logger.LogInformation("Finished write");

        return Ok();
    }

    private async Task<string> GetLinks(string url)
    {
        var args = new string[]{
            "./ScrapeScript/parser.py",
            //" -- ",
            $"{url}",
            "-v",
            "51",
            "-q",
            "720p"
        };
        using Process scraper = new Process();
        scraper.StartInfo.FileName = "python3";
        foreach (string arg in args)
            scraper.StartInfo.ArgumentList.Add(arg);
        scraper.StartInfo.RedirectStandardOutput = true;
        scraper.Start();
        await scraper.WaitForExitAsync();
        string? output = await scraper.StandardOutput.ReadLineAsync();
        if (output.Equals("--playlist=-"))
            output = await scraper.StandardOutput.ReadLineAsync();
        return output;
    }

}
