using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace VocabMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;

    public AccountController(IAccountService accountService, IMapper mapper)
    {
        _accountService = accountService;
        _mapper = mapper;
    }

    // API Login POST - Cập nhật để sử dụng JWT
    [HttpPost("login")]
    [Produces("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tokenResponse = await _accountService.Login(model.Name, model.Password);
        if (tokenResponse != null)
        {
            return Ok(tokenResponse);
        }

        return Unauthorized(new { message = "Invalid username or password" });
    }

    // API Register POST
    [HttpPost("register")]
    [Produces("application/json")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
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

    // API Logout - JWT không cần endpoint logout nhưng giữ lại để tương thích
    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT không cần server-side logout
        return Ok(new { success = true, message = "Logout successful" });
    }

    // API Get Current User
    [HttpGet("currentuser")]
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

    // API Refresh Token - Tạo token mới từ user hiện tại
    [HttpGet("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var user = await _accountService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized();
        }

        var tokenResponse = await _accountService.GenerateJwtToken(user);
        return Ok(tokenResponse);
    }
}
