using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepo _userRepository; // User repository
    private readonly IHttpContextAccessor _httpContextAccessor; // Http context accessor
    private readonly IMapper _mapper; // AutoMapper
    private readonly IConfiguration _configuration; // Configuration
    private readonly IHttpClientFactory _httpClientFactory; // Http client factory

    // Constructor
    public AccountService(IUserRepo userRepository,
                         IHttpContextAccessor httpContextAccessor,
                         IMapper mapper,
                         IConfiguration configuration,
                         IHttpClientFactory httpClientFactory)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    // Login - Cập nhật để sử dụng JWT
    public async Task<TokenResponseDto> Login(string name, string password)
    {
        var user = await _userRepository.ValidateUser(name, password); // ValidateUser method in UserRepo
        if (user != null) // If user exists
        {
            // Tạo JWT token
            return await GenerateJwtToken(user);
        }

        return null;
    }

    // Tạo JWT token
    public async Task<TokenResponseDto> GenerateJwtToken(User user)
    {
        // Lấy cấu hình JWT từ appsettings.json
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = jwtSettings["Secret"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryInDays = int.Parse(jwtSettings["ExpiryInDays"]);

        // Tạo claims cho token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("UserId", user.Id.ToString())
        };

        // Tạo key và credentials
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Tạo token
        var tokenExpiration = DateTime.UtcNow.AddDays(expiryInDays);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: tokenExpiration,
            signingCredentials: credentials
        );

        // Trả về token response
        return new TokenResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresIn = (int)(tokenExpiration - DateTime.UtcNow).TotalSeconds,
            UserId = user.Id,
            UserName = user.Name,
            Role = user.Role.ToString()
        };
    }

    // Xác thực người dùng Google
    public async Task<TokenResponseDto> AuthenticateGoogleUser(GoogleAuthDto googleAuth)
    {
        try
        {
            // Xử lý token trống hoặc null
            if (googleAuth == null || string.IsNullOrEmpty(googleAuth.AccessToken))
            {
                Console.WriteLine("Error: GoogleAuthDto or AccessToken is null/empty");
                return null;
            }

            Console.WriteLine($"Starting Google authentication with token length: {googleAuth.AccessToken.Length}");

            // Lấy thông tin người dùng từ Google
            var googleUserInfo = await GetGoogleUserInfo(googleAuth.AccessToken);

            if (googleUserInfo == null)
            {
                Console.WriteLine("Error: Failed to get Google user info");
                return null;
            }

            // Log thông tin user nhận được từ Google
            Console.WriteLine($"Google user info received: {googleUserInfo}");
            Console.WriteLine($"Email: {googleUserInfo.Email}, Name: {googleUserInfo.Name}");

            // Nếu không có email thì không thể xử lý
            if (string.IsNullOrEmpty(googleUserInfo.Email))
            {
                Console.WriteLine("Error: Google user email is missing");
                return null;
            }

            // Kiểm tra xem người dùng đã tồn tại trong hệ thống chưa
            var existingUser = await _userRepository.GetByName(googleUserInfo.Email);
            Console.WriteLine($"Existing user found: {existingUser != null}");

            if (existingUser == null)
            {
                // Tạo người dùng mới nếu chưa tồn tại
                var newUser = new User
                {
                    Name = googleUserInfo.Email, // Sử dụng email làm tên đăng nhập
                    Password = HashPassword(Guid.NewGuid().ToString()), // Tạo mật khẩu ngẫu nhiên
                    Role = UserRole.User
                };

                Console.WriteLine($"Creating new user with email: {newUser.Name}");
                await _userRepository.Add(newUser);
                existingUser = await _userRepository.GetByName(googleUserInfo.Email);

                if (existingUser == null)
                {
                    Console.WriteLine("Error: Failed to create new user");
                    return null;
                }
            }

            // Tạo JWT token cho người dùng
            Console.WriteLine($"Generating JWT token for user ID: {existingUser.Id}");
            var tokenResponse = await GenerateJwtToken(existingUser);
            Console.WriteLine("JWT token generated successfully");

            return tokenResponse;
        }
        catch (Exception ex)
        {
            // Log lỗi chi tiết
            Console.WriteLine($"Error authenticating Google user: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    // Lấy thông tin người dùng từ Google
    public async Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Error: AccessToken is null or empty");
                return null;
            }

            Console.WriteLine($"Getting user info with token length: {accessToken.Length}");
            Console.WriteLine($"Token preview: {accessToken.Substring(0, Math.Min(20, accessToken.Length))}...");

            var httpClient = _httpClientFactory.CreateClient("GoogleApi");

            // Thử nhiều cách gọi API khác nhau

            // Cách 1: Sử dụng Authorization header
            var url1 = "https://www.googleapis.com/oauth2/v3/userinfo";
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            Console.WriteLine($"Trying method 1 with URL: {url1}");
            var response1 = await httpClient.GetAsync(url1);
            var content1 = await response1.Content.ReadAsStringAsync();

            Console.WriteLine($"Method 1 response status: {response1.StatusCode}");
            Console.WriteLine($"Method 1 response content: {content1}");

            if (response1.IsSuccessStatusCode)
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var userInfo = JsonSerializer.Deserialize<GoogleUserInfoDto>(content1, options);

                if (userInfo == null)
                {
                    Console.WriteLine("Failed to deserialize Google user info");
                    return null;
                }

                Console.WriteLine($"Successfully obtained user info for: {userInfo.Email}");
                return userInfo;
            }

            // Cách 2: Sử dụng query parameter
            httpClient.DefaultRequestHeaders.Authorization = null;
            var url2 = $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}";

            Console.WriteLine($"Trying method 2 with URL parameter");
            var response2 = await httpClient.GetAsync(url2);
            var content2 = await response2.Content.ReadAsStringAsync();

            Console.WriteLine($"Method 2 response status: {response2.StatusCode}");
            Console.WriteLine($"Method 2 response content: {content2}");

            if (response2.IsSuccessStatusCode)
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var userInfo = JsonSerializer.Deserialize<GoogleUserInfoDto>(content2, options);

                if (userInfo == null)
                {
                    Console.WriteLine("Failed to deserialize Google user info");
                    return null;
                }

                Console.WriteLine($"Successfully obtained user info for: {userInfo.Email}");
                return userInfo;
            }

            // Cả hai cách đều thất bại
            Console.WriteLine("Both methods failed to get user info from Google");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Google user info: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    // Create user claims
    private List<Claim> CreateUserClaims(User user)
    {
        return new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("UserId", user.Id.ToString())
        };
    }

    private AuthenticationProperties CreateAuthenticationProperties()
    {
        return new AuthenticationProperties
        {
            IsPersistent = true // Keep the user logged in even after the browser is closed
        };
    }

    public async Task<bool> Register(User user)
    {
        if (await _userRepository.IsNameExist(user.Name)) // IsNameExist method in UserRepo
            return false;

        user.Password = HashPassword(user.Password);

        await _userRepository.Add(user); // Add method in UserRepo
        return true;
    }

    public async Task Logout()
    {
        // Không cần thực hiện gì với JWT vì token được lưu ở client
        // JWT sẽ hết hạn theo thời gian đã cấu hình
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public async Task<User> GetCurrentUser()
    {
        if (_httpContextAccessor.HttpContext == null || !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            return null;
        }

        var userId = _httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
        {
            return null;
        }

        var user = await _userRepository.GetById(id);
        return user;
    }
}


