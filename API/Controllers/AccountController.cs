using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Service.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AccountController(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(User model)
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
    public async Task<IActionResult> GoogleLogin([FromBody] Dictionary<string, string> googleAuth)
    {
        try
        {
            if (googleAuth == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            if (!googleAuth.TryGetValue("accessToken", out var accessToken) || string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("AccessToken không được cung cấp");
            }

            try
            {
                var tokenResponse = await _authenticationService.AuthenticateGoogleUser(accessToken);

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
    public async Task<IActionResult> ValidateGoogleToken([FromBody] Dictionary<string, string> googleAuth)
    {

        try
        {
            if (googleAuth == null || !googleAuth.TryGetValue("accessToken", out var accessToken) || string.IsNullOrEmpty(accessToken))
            {
                return BadRequest(new { valid = false, message = "Token is missing" });
            }

            var userInfo = await _authenticationService.GetGoogleUserInfo(accessToken);

            if (userInfo != null)
            {
                userInfo.TryGetValue("sub", out var id);
                userInfo.TryGetValue("email", out var email);
                userInfo.TryGetValue("name", out var name);
                userInfo.TryGetValue("picture", out var picture);
                userInfo.TryGetValue("email_verified", out var emailVerified);

                return Ok(new
                {
                    valid = true,
                    userInfo = new
                    {
                        id = id,
                        email = email,
                        name = name,
                        picture = picture,
                        emailVerified = emailVerified
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
    public async Task<IActionResult> Register(User model)
    {
        try
        {
            model.Role = UserRole.User;
            var result = await _authenticationService.Register(model);
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
