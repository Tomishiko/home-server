using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mvc_server.Models;
using mvc_server.Models;
using mvc_server.Services;

namespace mvc_server.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICoreFS _coreFS;
    public HomeController(ILogger<HomeController> logger, ICoreFS coreFS)
    {
        _logger = logger;
        _coreFS = coreFS;
    }
    public IActionResult Index()
    {
        string[] items = Directory.GetFiles("wwwroot/files");
        FileInfo[] files = new FileInfo[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            files[i] = new FileInfo(items[i]);
        }
        //TODO:  file system caching
        return View(files);
    }
    [Route("/movies")]
    public IActionResult Movies()
    {

        return View(_coreFS.GetMovies);

    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
