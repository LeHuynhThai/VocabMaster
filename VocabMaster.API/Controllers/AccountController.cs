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
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthenticationService authenticationService,
        ITokenService tokenService,
        IExternalAuthService externalAuthService,
        IMapper mapper,
        ILogger<AccountController> logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _externalAuthService = externalAuthService ?? throw new ArgumentNullException(nameof(externalAuthService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Login
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

    // Google Login
    [HttpPost("google-login")]
    [Produces("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto googleAuth)
    {
        try
        {
            Console.WriteLine("Google Login API endpoint được gọi");

            if (googleAuth == null)
            {
                Console.WriteLine("GoogleAuthDto là null");
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Debug
            Console.WriteLine($"GoogleAuth: accessToken length={googleAuth.AccessToken?.Length ?? 0}, idToken length={googleAuth.IdToken?.Length ?? 0}");

            if (string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                Console.WriteLine("AccessToken là null hoặc rỗng");
                return BadRequest("AccessToken không được cung cấp");
            }

            // IdToken is optional
            if (string.IsNullOrEmpty(googleAuth.IdToken))
            {
                Console.WriteLine("IdToken không được cung cấp, nhưng tiếp tục xử lý với AccessToken");
            }

            // Debug
            Console.WriteLine($"Token preview: {googleAuth.AccessToken.Substring(0, Math.Min(20, googleAuth.AccessToken.Length))}...");

            try
            {
                var tokenResponse = await _externalAuthService.AuthenticateGoogleUser(googleAuth);

                if (tokenResponse == null)
                {
                    Console.WriteLine("Không nhận được token response từ AuthenticateGoogleUser");
                    return Unauthorized("Không thể xác thực với Google");
                }

                Console.WriteLine($"Xác thực thành công cho user: {tokenResponse.UserName}");
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xác thực Google: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }

                return StatusCode(500, $"Lỗi xác thực Google: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi không xác định trong GoogleLogin: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, "Đã xảy ra lỗi không xác định");
        }
    }

    // Validate Google Token
    [HttpPost("validate-google-token")]
    [Produces("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateGoogleToken([FromBody] GoogleAuthDto googleAuth)
    {
        _logger.LogInformation("ValidateGoogleToken endpoint called");

        try
        {
            if (googleAuth == null || string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                return BadRequest(new { valid = false, message = "Token is missing" });
            }

            // Get user info from Google, not create JWT
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
            _logger.LogError(ex, "Error validating Google token");
            return StatusCode(500, new { valid = false, message = ex.Message });
        }
    }

    // Register
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
            return Ok(new { success = true, message = "Registration successful" });
        }

        return BadRequest(new { message = "Username already exists" });
    }

    // Logout
    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT doesn't need server-side logout
        return Ok(new { success = true, message = "Logout successful" });
    }

    // Get Current User
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

    // Refresh Token
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

    // Test
    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        Console.WriteLine("Test endpoint được gọi");
        return Ok(new { message = "API đang hoạt động", time = DateTime.Now });
    }
}
