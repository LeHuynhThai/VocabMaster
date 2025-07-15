using Microsoft.AspNetCore.Mvc;
using VocabMaster.Models.User;
using VocabMaster.Services.Interfaces;
using VocabMaster.Entities;

namespace VocabMaster.Controllers;

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
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _accountService.LoginAsync(model.Name, model.Password);
        if (user != null)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Username or password is incorrect");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create user
        var user = new User 
        { 
            Name = model.Name, // Set name
            Password = model.Password // Set password
        };

        // Register user
        if (await _accountService.RegisterAsync(user))
        {
            return RedirectToAction("Login"); // Redirect to login page
        }

        ModelState.AddModelError("", "Username already exists");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }
}
