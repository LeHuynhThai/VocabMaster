using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

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

    // MVC View Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // MVC Login POST
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

    // API Login POST
    [HttpPost("api/account/login")]
    [Produces("application/json")]
    public async Task<IActionResult> ApiLogin([FromBody] LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _accountService.Login(model.Name, model.Password);
        if (user != null)
        {
            return Ok(new { 
                id = user.Id, 
                name = user.Name, 
                role = user.Role.ToString() 
            });
        }

        return Unauthorized(new { message = "Invalid username or password" });
    }

    // MVC Register View
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // MVC Register POST
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

    // API Register POST
    [HttpPost("api/account/register")]
    [Produces("application/json")]
    public async Task<IActionResult> ApiRegister([FromBody] RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = _mapper.Map<User>(model);

        if (await _accountService.Register(user))
        {
            return Ok(new { success = true, message = "Registration successful" });
        }

        return BadRequest(new { message = "Username already exists" });
    }

    // MVC Logout
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _accountService.Logout();
        return RedirectToAction("Login");
    }

    // API Logout
    [HttpGet("api/account/logout")]
    [Authorize]
    public async Task<IActionResult> ApiLogout()
    {
        await _accountService.Logout();
        return Ok(new { success = true });
    }

    // API Get Current User
    [HttpGet("api/account/currentuser")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _accountService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new {
            id = user.Id,
            name = user.Name,
            role = user.Role.ToString(),
            learnedWordsCount = user.LearnedVocabularies?.Count ?? 0
        });
    }
}
