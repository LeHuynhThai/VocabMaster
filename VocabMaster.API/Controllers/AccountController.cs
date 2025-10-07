using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;
    private readonly IExternalAuthService _externalAuthService;
    private readonly IMapper _mapper;

    public AccountController(
        IAuthenticationService authenticationService,
        ITokenService tokenService,
        IExternalAuthService externalAuthService,
        IMapper mapper)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _externalAuthService = externalAuthService ?? throw new ArgumentNullException(nameof(externalAuthService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpPost("login")]
    [Produces("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var tokenResponse = await _authenticationService.Login(model.Name, model.Password);
        if (tokenResponse != null)
        {
            return Ok(tokenResponse);
        }

        return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không hợp lệ" });
    }

    [HttpPost("google-login")]
    [Produces("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto googleAuth)
    {
        try
        {
            if (googleAuth == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            if (string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                return BadRequest("AccessToken không được cung cấp");
            }

            try
            {
                var tokenResponse = await _externalAuthService.AuthenticateGoogleUser(googleAuth);

                if (tokenResponse == null)
                {
                    return Unauthorized("Không thể xác thực với Google");
                }

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xác thực Google: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Đã xảy ra lỗi không xác định");
        }
    }

    [HttpPost("validate-google-token")]
    [Produces("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateGoogleToken([FromBody] GoogleAuthDto googleAuth)
    {

        try
        {
            if (googleAuth == null || string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                return BadRequest(new { valid = false, message = "Token is missing" });
            }

            var userInfo = await _externalAuthService.GetGoogleUserInfo(googleAuth.AccessToken);

            if (userInfo != null)
            {
                return Ok(new
                {
                    valid = true,
                    userInfo = new
                    {
                        id = userInfo.Id,
                        email = userInfo.Email,
                        name = userInfo.Name,
                        picture = userInfo.Picture,
                        emailVerified = userInfo.EmailVerified
                    }
                });
            }

            return BadRequest(new { valid = false, message = "Invalid or expired token" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { valid = false, message = ex.Message });
        }
    }

    [HttpPost("register")]
    [Produces("application/json")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = _mapper.Map<User>(model);

        if (await _authenticationService.Register(user))
        {
            return Ok(new { success = true, message = "Đăng ký thành công" });
        }

        return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
    }

    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { success = true, message = "Đăng xuất thành công" });
    }

    [HttpGet("currentuser")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _authenticationService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            role = user.Role.ToString(),
            learnedWordsCount = user.LearnedVocabularies?.Count ?? 0
        });
    }
                
    [HttpGet("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var user = await _authenticationService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized();
        }

        var tokenResponse = await _tokenService.GenerateJwtToken(user);
        return Ok(tokenResponse);
    }

    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        return Ok(new { message = "API đang hoạt động", time = DateTime.Now });
    }
}
