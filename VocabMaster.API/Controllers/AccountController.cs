using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;

    public AccountController(IAccountService accountService, IMapper mapper)
    {
        _accountService = accountService;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _accountService.Login(model.Name, model.Password);
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
    public async Task<IActionResult> Register(RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Sử dụng AutoMapper để map từ RegisterRequestDto sang User
        var user = _mapper.Map<User>(model);

        // Register user
        if (await _accountService.Register(user))
        {
            TempData["SuccessMessage"] = "Register successfully";
            return RedirectToAction("Login"); // Redirect to login page
        }

        ModelState.AddModelError("", "Username already exists");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _accountService.Logout();
        return RedirectToAction("Login");
    }
}
