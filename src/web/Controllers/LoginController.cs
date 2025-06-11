using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

public class LoginController : Controller
{

    public IActionResult Index([FromHeader(Name="X-Requested-With")] string requestWith)
    {
        return requestWith switch{
            "XMLHttpRequest" => PartialView(),
            _ => View()
        };
    }
}
