using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Entities;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Controllers
{
    public class AccountController : Controller
    {

        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login
        [HttpPost]
        public async Task<IActionResult> Login(string name)
        {
            if (await _accountService.LoginAsync(name)) // If login successful
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Login failed"; // display error
                return View();
            }
        }

        // Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (await _accountService.RegisterAsync(user)) // If register successful
            {
                ViewBag.Success = "Register successful"; // display success
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("Name", "Name already exists"); // display error   
                return View();
            }
        }

        // Logout
        public IActionResult Logout()
        {
            _accountService.Logout();
            return RedirectToAction("Login");
        }
    }
}
