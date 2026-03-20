using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace web.ViewModels;

public class HomeViewModel : PageModel
{
    public IEnumerable<FileInfo>? Files { get; set; }
    [BindProperty]
    public IFormFile? Upload { get; set; }

}
