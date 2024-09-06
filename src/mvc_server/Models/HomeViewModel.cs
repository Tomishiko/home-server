using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace mvc_server.Models;

public class HomeViewModel : PageModel
{
    public IEnumerable<FileInfo> Files { get; set; }
    [BindProperty]
    public IFormFile Upload { get; set; }

}
