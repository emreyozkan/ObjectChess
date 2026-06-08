using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;
using ObjectChess.Web.ViewModels;

namespace ObjectChess.Web.Controllers;

public class AuthController(IAuthService authService) : Controller
{
    private const string AuthScheme = "ObjectChessAuth";

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        // If the form broke a rule then show it again with the errors
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            authService.Register(model.FullName, model.Email, model.Password);
            // Registration worked so send them to the login page
            return RedirectToAction("Login");
        }
        catch (ArgumentException ex)
        {
            // The service threw a rule error so show that message on the form
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        UserModel? user = authService.Login(model.Email, model.Password);

        // No matching user so show a vague error on purpose and do not say which part was wrong
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Claims are the bits of info we want to remember about the logged in user
        // They get saved inside the cookie
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email)
        ];

        // Wrap the claims up into the user object that ASP.NET understands
        ClaimsIdentity identity = new(claims, AuthScheme);
        ClaimsPrincipal principal = new(identity);

        AuthenticationProperties properties = new()
        {
            // Keep them logged in even after closing the browser
            // But the cookie still expires after 2 hours
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        };

        // This is what actually writes the login cookie to the browser
        await HttpContext.SignInAsync(AuthScheme, principal, properties);

        return RedirectToAction("MatchHistory", "Match");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Delete the login cookie so the user is signed out
        await HttpContext.SignOutAsync(AuthScheme);
        return RedirectToAction("Login");
    }
}
