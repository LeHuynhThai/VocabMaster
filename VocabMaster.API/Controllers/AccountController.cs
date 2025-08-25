using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers;

/// <summary>
/// Controller xử lý các chức năng liên quan đến tài khoản người dùng
/// Bao gồm: đăng nhập, đăng ký, xác thực Google, quản lý token
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    // Các service được inject thông qua Dependency Injection
    private readonly IAuthenticationService _authenticationService; // Service xử lý xác thực người dùng
    private readonly ITokenService _tokenService; // Service tạo và quản lý JWT token
    private readonly IExternalAuthService _externalAuthService; // Service xác thực với bên thứ 3 (Google)
    private readonly IMapper _mapper; // AutoMapper để chuyển đổi giữa các object
    private readonly ILogger<AccountController> _logger; // Logger để ghi log

    /// <summary>
    /// Constructor - Khởi tạo AccountController với các dependency cần thiết
    /// </summary>
    /// <param name="authenticationService">Service xử lý xác thực</param>
    /// <param name="tokenService">Service quản lý token</param>
    /// <param name="externalAuthService">Service xác thực bên ngoài</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="logger">Logger instance</param>
    public AccountController(
        IAuthenticationService authenticationService,
        ITokenService tokenService,
        IExternalAuthService externalAuthService,
        IMapper mapper,
        ILogger<AccountController> logger)
    {
        // Kiểm tra null và gán giá trị cho các field
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _externalAuthService = externalAuthService ?? throw new ArgumentNullException(nameof(externalAuthService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// API đăng nhập bằng tài khoản thông thường (username/password)
    /// </summary>
    /// <param name="model">Thông tin đăng nhập (tên đăng nhập và mật khẩu)</param>
    /// <returns>JWT token nếu đăng nhập thành công, lỗi nếu thất bại</returns>
    [HttpPost("login")]
    [Produces("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        // Kiểm tra tính hợp lệ của dữ liệu đầu vào
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Gọi service để xác thực người dùng và tạo token
        var tokenResponse = await _authenticationService.Login(model.Name, model.Password);
        if (tokenResponse != null)
        {
            return Ok(tokenResponse); // Trả về token nếu đăng nhập thành công
        }

        // Trả về lỗi 401 nếu thông tin đăng nhập không đúng
        return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không hợp lệ" });
    }

    /// <summary>
    /// API đăng nhập bằng tài khoản Google
    /// Xử lý xác thực với Google OAuth và tạo JWT token cho hệ thống
    /// </summary>
    /// <param name="googleAuth">Thông tin xác thực từ Google (AccessToken và IdToken)</param>
    /// <returns>JWT token của hệ thống nếu xác thực thành công</returns>
    [HttpPost("google-login")]
    [Produces("application/json")]
    [AllowAnonymous] // Cho phép truy cập mà không cần xác thực
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto googleAuth)
    {
        try
        {
            // Ghi log để theo dõi việc gọi API
            Console.WriteLine("Google Login API endpoint được gọi");

            // Kiểm tra dữ liệu đầu vào có null không
            if (googleAuth == null)
            {
                Console.WriteLine("GoogleAuthDto là null");
                return BadRequest("Dữ liệu không hợp lệ");
            }

            // Ghi log thông tin debug về token
            Console.WriteLine($"GoogleAuth: accessToken length={googleAuth.AccessToken?.Length ?? 0}, idToken length={googleAuth.IdToken?.Length ?? 0}");

            // Kiểm tra AccessToken có tồn tại không (bắt buộc)
            if (string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                Console.WriteLine("AccessToken là null hoặc rỗng");
                return BadRequest("AccessToken không được cung cấp");
            }

            // IdToken là tùy chọn, không bắt buộc
            if (string.IsNullOrEmpty(googleAuth.IdToken))
            {
                Console.WriteLine("IdToken không được cung cấp, nhưng tiếp tục xử lý với AccessToken");
            }

            // Ghi log một phần của token để debug (chỉ 20 ký tự đầu)
            Console.WriteLine($"Token preview: {googleAuth.AccessToken.Substring(0, Math.Min(20, googleAuth.AccessToken.Length))}...");

            try
            {
                // Gọi service để xác thực với Google và tạo JWT token cho hệ thống
                var tokenResponse = await _externalAuthService.AuthenticateGoogleUser(googleAuth);

                if (tokenResponse == null)
                {
                    Console.WriteLine("Không nhận được token response từ AuthenticateGoogleUser");
                    return Unauthorized("Không thể xác thực với Google");
                }

                Console.WriteLine($"Xác thực thành công cho user: {tokenResponse.UserName}");
                return Ok(tokenResponse); // Trả về JWT token của hệ thống
            }
            catch (Exception ex)
            {
                // Xử lý lỗi trong quá trình xác thực Google
                Console.WriteLine($"Lỗi xác thực Google: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Ghi log chi tiết nếu có inner exception
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
            // Xử lý các lỗi không mong muốn khác
            Console.WriteLine($"Lỗi không xác định trong GoogleLogin: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, "Đã xảy ra lỗi không xác định");
        }
    }

    /// <summary>
    /// API kiểm tra tính hợp lệ của Google token và lấy thông tin người dùng
    /// Không tạo JWT token mà chỉ trả về thông tin người dùng từ Google
    /// </summary>
    /// <param name="googleAuth">Thông tin xác thực Google chứa AccessToken</param>
    /// <returns>Thông tin người dùng từ Google nếu token hợp lệ</returns>
    [HttpPost("validate-google-token")]
    [Produces("application/json")]
    [AllowAnonymous] // Cho phép truy cập mà không cần xác thực
    public async Task<IActionResult> ValidateGoogleToken([FromBody] GoogleAuthDto googleAuth)
    {
        // Ghi log việc gọi endpoint này
        _logger.LogInformation("ValidateGoogleToken endpoint called");

        try
        {
            // Kiểm tra dữ liệu đầu vào
            if (googleAuth == null || string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                return BadRequest(new { valid = false, message = "Token is missing" });
            }

            // Lấy thông tin người dùng từ Google (không tạo JWT token)
            var userInfo = await _externalAuthService.GetGoogleUserInfo(googleAuth.AccessToken);

            if (userInfo != null)
            {
                // Trả về thông tin người dùng nếu token hợp lệ
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

            // Token không hợp lệ hoặc đã hết hạn
            return BadRequest(new { valid = false, message = "Invalid or expired token" });
        }
        catch (Exception ex)
        {
            // Ghi log lỗi và trả về thông báo lỗi
            _logger.LogError(ex, "Error validating Google token");
            return StatusCode(500, new { valid = false, message = ex.Message });
        }
    }

    /// <summary>
    /// API đăng ký tài khoản mới cho người dùng
    /// </summary>
    /// <param name="model">Thông tin đăng ký (tên đăng nhập, mật khẩu, email, v.v.)</param>
    /// <returns>Thông báo thành công hoặc lỗi</returns>
    [HttpPost("register")]
    [Produces("application/json")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        // Kiểm tra tính hợp lệ của dữ liệu đầu vào
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Chuyển đổi DTO thành entity User bằng AutoMapper
        var user = _mapper.Map<User>(model);

        // Gọi service để đăng ký người dùng
        if (await _authenticationService.Register(user))
        {
            return Ok(new { success = true, message = "Đăng ký thành công" });
        }

        // Trả về lỗi nếu tên đăng nhập đã tồn tại
        return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
    }

    /// <summary>
    /// API đăng xuất người dùng
    /// Với JWT token, không cần xử lý đăng xuất phía server
    /// Client chỉ cần xóa token khỏi storage
    /// </summary>
    /// <returns>Thông báo đăng xuất thành công</returns>
    [HttpGet("logout")]
    [Authorize] // Yêu cầu người dùng phải đăng nhập
    public IActionResult Logout()
    {
        // JWT không cần xử lý đăng xuất phía server
        // Client sẽ tự xóa token khỏi localStorage/sessionStorage
        return Ok(new { success = true, message = "Đăng xuất thành công" });
    }

    /// <summary>
    /// API lấy thông tin người dùng hiện tại
    /// Dựa vào JWT token trong header Authorization
    /// </summary>
    /// <returns>Thông tin cơ bản của người dùng đang đăng nhập</returns>
    [HttpGet("currentuser")]
    [Authorize] // Yêu cầu người dùng phải đăng nhập
    public async Task<IActionResult> GetCurrentUser()
    {
        // Lấy thông tin người dùng từ JWT token
        var user = await _authenticationService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized(); // Token không hợp lệ hoặc người dùng không tồn tại
        }

        // Trả về thông tin cơ bản của người dùng
        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            role = user.Role.ToString(),
            learnedWordsCount = user.LearnedVocabularies?.Count ?? 0 // Số từ vựng đã học
        });
    }

    /// <summary>
    /// API làm mới JWT token
    /// Tạo token mới cho người dùng hiện tại để kéo dài thời gian đăng nhập
    /// </summary>
    /// <returns>JWT token mới</returns>
    [HttpGet("refresh-token")]
    [Authorize] // Yêu cầu người dùng phải đăng nhập
    public async Task<IActionResult> RefreshToken()
    {
        // Lấy thông tin người dùng từ token hiện tại
        var user = await _authenticationService.GetCurrentUser();
        if (user == null)
        {
            return Unauthorized(); // Token không hợp lệ
        }

        // Tạo JWT token mới cho người dùng
        var tokenResponse = await _tokenService.GenerateJwtToken(user);
        return Ok(tokenResponse);
    }

    /// <summary>
    /// API test để kiểm tra API có hoạt động không
    /// Dùng cho mục đích debug và health check
    /// </summary>
    /// <returns>Thông báo API đang hoạt động và thời gian hiện tại</returns>
    [HttpGet("test")]
    [AllowAnonymous] // Không yêu cầu xác thực
    public IActionResult Test()
    {
        Console.WriteLine("Test endpoint được gọi");
        return Ok(new { message = "API đang hoạt động", time = DateTime.Now });
    }
}
