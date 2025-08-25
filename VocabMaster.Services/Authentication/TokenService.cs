using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services.Authentication
{
    // Service tạo và xác thực JWT token cho user
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        // Hàm khởi tạo service, inject config và logger
        public TokenService(
            IConfiguration configuration,
            ILogger<TokenService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Sinh JWT token cho user
        public async Task<TokenResponseDto> GenerateJwtToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var jwtSettings = _configuration.GetSection("JWT");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryInDays = int.Parse(jwtSettings["ExpiryInDays"]);

            var claims = CreateUserClaims(user);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenExpiration = DateTime.UtcNow.AddDays(expiryInDays);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: tokenExpiration,
                signingCredentials: credentials
            );

            return new TokenResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = (int)(tokenExpiration - DateTime.UtcNow).TotalSeconds,
                UserId = user.Id,
                UserName = user.Name,
                Role = user.Role.ToString()
            };
        }

        // Tạo danh sách claim cho user
        public List<Claim> CreateUserClaims(User user)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim("sub", user.Id.ToString())
            };
        }

        // Lấy userId từ claim trong JWT
        public string FindUserIdFromClaims(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("UserId")?.Value
                ?? user.FindFirst("userId")?.Value
                ?? user.FindFirst("id")?.Value
                ?? user.FindFirst("sub")?.Value;
        }
    }
}