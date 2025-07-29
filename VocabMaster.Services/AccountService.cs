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

namespace VocabMaster.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepo _userRepository; // User repository
    private readonly IHttpContextAccessor _httpContextAccessor; // Http context accessor
    private readonly IMapper _mapper; // AutoMapper
    private readonly IConfiguration _configuration; // Configuration

    // Constructor
    public AccountService(IUserRepo userRepository,
                         IHttpContextAccessor httpContextAccessor,
                         IMapper mapper,
                         IConfiguration configuration)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _configuration = configuration;
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


