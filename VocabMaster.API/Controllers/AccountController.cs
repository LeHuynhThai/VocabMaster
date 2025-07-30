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
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, IMapper mapper, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _mapper = mapper;
        _logger = logger;
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

    // API Google Login
    [HttpPost("google-login")]
    [Produces("application/json")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto googleAuth)
    {
        try
        {
            if (googleAuth == null || string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                return BadRequest(new { message = "Invalid Google authentication data" });
            }

            _logger.LogInformation("Processing Google login with token: {Token}", googleAuth.AccessToken.Substring(0, 10) + "...");

            var tokenResponse = await _accountService.AuthenticateGoogleUser(googleAuth);
            if (tokenResponse != null)
            {
                _logger.LogInformation("Google login successful for user: {UserName}", tokenResponse.UserName);
                return Ok(tokenResponse);
            }

            _logger.LogWarning("Google login failed - unable to authenticate user");
            return Unauthorized(new { message = "Unable to authenticate with Google" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return StatusCode(500, new { message = "An error occurred during Google authentication" });
        }
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
