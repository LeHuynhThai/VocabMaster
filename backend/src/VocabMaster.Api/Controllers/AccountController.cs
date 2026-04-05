using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Api.Contracts.Account;
using VocabMaster.Api.Contracts.Common;
using VocabMaster.Application.Interfaces;

namespace VocabMaster.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthenticationService authenticationService,
        ICurrentUserService currentUserService,
        ILogger<AccountController> logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var loginResult = await _authenticationService.Login(model.Name, model.Password);
            if (loginResult is not null)
            {
                return Ok(loginResult);
            }

            return Unauthorized(new AuthenticationErrorResponse { Message = "Tên đăng nhập hoặc mật khẩu không hợp lệ" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while logging in user {UserName}", model.Name);
            return StatusCode(500, new MessageResponse { Message = "Đã xảy ra lỗi khi đăng nhập" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _authenticationService.Register(model.Name, model.Password);
            if (result)
            {
                return Ok(new OperationResultResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công"
                });
            }

            return BadRequest(new MessageResponse { Message = "Tên đăng nhập đã tồn tại" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while registering user {UserName}", model.Name);
            return StatusCode(500, new MessageResponse { Message = "Đã xảy ra lỗi khi đăng ký" });
        }
    }

    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new OperationResultResponse
        {
            Success = true,
            Message = "Đăng xuất thành công"
        });
    }

    [HttpGet("currentuser")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _currentUserService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse
        {
            Id = user.Id,
            Name = user.Name,
            LearnedWordsCount = user.LearnedVocabularies?.Count ?? 0
        });
    }

    [HttpGet("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var user = await _currentUserService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized();
        }

        var tokenResponse = await _authenticationService.GenerateJwtToken(user);
        return Ok(tokenResponse);
    }
}
