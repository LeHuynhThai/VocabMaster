using Microsoft.AspNetCore.Mvc;
using VocabMaster.Entities;
using VocabMaster.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace VocabMaster.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Please enter all login information");
                return View();
            }

            var user = await _accountService.LoginAsync(name, password);
            if (user != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Username or password is incorrect");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (await _accountService.RegisterAsync(user))
            {
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("Name", "Username already exists");
            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Login");
        }
    }
}
