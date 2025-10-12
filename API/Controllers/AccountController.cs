using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.DTOs;
using Repository.Entities;
using Service.Interfaces;

namespace API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMapper _mapper;

    public AccountController(
        IAuthenticationService authenticationService,
        IMapper mapper)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto model)
    {
        // validate fields login request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var loginResult = await _authenticationService.Login(model.Name, model.Password);
            if (loginResult != null)
            {
                return Ok(loginResult);
            }
            else
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không hợp lệ" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new { message = "Đã xảy ra lỗi khi đăng nhập" });
        }
    }

    [HttpPost("google-login")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin(GoogleAuthDto googleAuth)
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
                var tokenResponse = await _authenticationService.AuthenticateGoogleUser(googleAuth);

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
    [AllowAnonymous]
    public async Task<IActionResult> ValidateGoogleToken(GoogleAuthDto googleAuth)
    {

        try
        {
            if (googleAuth == null || string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                return BadRequest(new { valid = false, message = "Token is missing" });
            }

            var userInfo = await _authenticationService.GetGoogleUserInfo(googleAuth.AccessToken);

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
    public async Task<IActionResult> Register(RegisterRequestDto model)
    {
        try
        {
            var user = new User
            {
                Name = model.Name,
                Password = model.Password,
                Role = UserRole.User
            };
            var result = await _authenticationService.Register(user);
            if (result)
            {
             return Ok(new { success = true, message = "Đăng ký thành công" });
            }
            else
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new { message = "Đã xảy ra lỗi khi đăng ký" });
        }
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

        var tokenResponse = await _authenticationService.GenerateJwtToken(user);
        return Ok(tokenResponse);
    }
}
