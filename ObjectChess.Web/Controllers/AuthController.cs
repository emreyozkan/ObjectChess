using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ObjectChess.Business.Interfaces;
using ObjectChess.Web.ViewModels;

namespace ObjectChess.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _authService.Register(model.FullName, model.Email, model.Password);
                return RedirectToAction("Login");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
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

            string? userFullName = _authService.Login(model.Email, model.Password);

            if (userFullName != null)
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userFullName),
                    new Claim(ClaimTypes.Email, model.Email)
                };

                ClaimsIdentity identity = new ClaimsIdentity(claims, "ObjectChessAuth");
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                
                AuthenticationProperties properties = new AuthenticationProperties
                {
                    IsPersistent = true, 
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync("ObjectChessAuth", principal, properties);

                return RedirectToAction("MatchHistory", "Match");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ObjectChessAuth");
            return RedirectToAction("Login");
        }
    }
}