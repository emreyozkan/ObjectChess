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
    // This controller handles user authentication (register, login, logout)
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        // Dependency injection for auth service
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Show register page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Handle register form submission
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            // Validate form inputs
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Call service to create user
                _authService.Register(model.FullName, model.Email, model.Password);

                // After success go to login page
                return RedirectToAction("Login");
            }
            catch (ArgumentException ex)
            {
                // Show error on email field
                ModelState.AddModelError("Email", ex.Message);
                return View(model);
            }
        }

        // Show login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Handle login request
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Validate input
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Try login (returns fullName if success)
            string? userFullName = _authService.Login(model.Email, model.Password);

            if (userFullName != null)
            {
                // Create claims (user identity info stored in cookie)
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userFullName), // IMPORTANT: using FullName as identity
                    new Claim(ClaimTypes.Email, model.Email)
                };

                // Create authentication identity
                ClaimsIdentity identity = new ClaimsIdentity(claims, "ObjectChessAuth");
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                // Set login session properties
                AuthenticationProperties properties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                };

                // Sign in user (create cookie)
                await HttpContext.SignInAsync("ObjectChessAuth", principal, properties);

                // Redirect to match history page
                return RedirectToAction("MatchHistory", "Match");
            }

            // Login failed
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Logout user
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ObjectChessAuth");
            return RedirectToAction("Login");
        }
    }
}