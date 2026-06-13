using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using ObjectChess.Web.ViewModels;

namespace ObjectChess.Web.Controllers;

[Authorize] 
public class HomeController : Controller
{
    // This controller only serves the error page now.
    // The exception handler in Program.cs points to /Home/Error so it has to stay.

    [AllowAnonymous] 
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}