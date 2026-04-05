using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;

namespace VocabMaster.Api.Controllers;

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
