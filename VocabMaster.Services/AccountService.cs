using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMapper;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VocabMaster.Core.DTOs;
using System.Net.Http;
using System.Text.Json;

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
            // Lấy thông tin người dùng từ Google
            var googleUserInfo = await GetGoogleUserInfo(googleAuth.AccessToken);
            
            if (googleUserInfo == null)
            {
                return null;
            }
            
            // Kiểm tra xem người dùng đã tồn tại trong hệ thống chưa
            var existingUser = await _userRepository.GetByName(googleUserInfo.Email);
            
            if (existingUser == null)
            {
                // Tạo người dùng mới nếu chưa tồn tại
                var newUser = new User
                {
                    Name = googleUserInfo.Email, // Sử dụng email làm tên đăng nhập
                    Password = HashPassword(Guid.NewGuid().ToString()), // Tạo mật khẩu ngẫu nhiên
                    Role = UserRole.User
                };
                
                await _userRepository.Add(newUser);
                existingUser = await _userRepository.GetByName(googleUserInfo.Email);
            }
            
            // Tạo JWT token cho người dùng
            return await GenerateJwtToken(existingUser);
        }
        catch (Exception ex)
        {
            // Log lỗi
            Console.WriteLine($"Error authenticating Google user: {ex.Message}");
            return null;
        }
    }
    
    // Lấy thông tin người dùng từ Google
    public async Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GoogleUserInfoDto>(content);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            // Log lỗi
            Console.WriteLine($"Error getting Google user info: {ex.Message}");
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


