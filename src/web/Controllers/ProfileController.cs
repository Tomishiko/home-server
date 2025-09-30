using Microsoft.AspNetCore.Mvc;
using core.Models;
using web.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Authorize]
public class ProfileController : Controller
{

    public IActionResult Index([FromHeader(Name = "X-Requested-With")] string requestWith)
    {

        var testUser = new ProfileViewModel
        {
            FullName = "Full Name",
            Username = "user name",
            ProfilePictureUrl = "",
            Bio = "This is my long bio",
            Email = "email@email.com",
            Phone = "+38095123678",
            Location = "why is there Location?"
        };
        return Utility.IsXmlHttpRequest(requestWith) ? PartialView(testUser) : View(testUser);
    }
}
