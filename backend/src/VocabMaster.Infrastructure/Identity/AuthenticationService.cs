using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
// Removed System.Text.Json usage as Google OAuth helpers were removed
using VocabMaster.Application.Interfaces;
using VocabMaster.Application.Models.Authentication;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;

namespace VocabMaster.Infrastructure.Identity
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepo _userRepository;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepo userRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AuthenticationResult?> Login(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await _userRepository.GetByName(name.Trim());
            if (user == null || !VerifyPassword(password, user.Password))
            {
                return null;
            }

            return await GenerateJwtToken(user);
        }

        public async Task<bool> Register(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var normalizedName = name.Trim();

            if (await _userRepository.IsNameExist(normalizedName))
                return false;

            var user = new User
            {
                Name = normalizedName,
                Password = HashPassword(password),
                Role = UserRole.User
            };

            await _userRepository.Add(user);
            return true;
        }

        public async Task Logout()
        {
            await Task.CompletedTask;
        }

        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash cannot be null or empty", nameof(hash));

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public async Task<AuthenticationResult> GenerateJwtToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var jwtSettings = _configuration.GetSection("JWT");
            var secretKey = GetRequiredJwtSetting(jwtSettings, "Secret");
            var issuer = GetRequiredJwtSetting(jwtSettings, "Issuer");
            var audience = GetRequiredJwtSetting(jwtSettings, "Audience");

            if (!int.TryParse(GetRequiredJwtSetting(jwtSettings, "ExpiryInDays"), out var expiryInDays))
            {
                throw new InvalidOperationException("JWT setting 'ExpiryInDays' must be a valid integer.");
            }

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

            return await Task.FromResult(new AuthenticationResult
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = (int)(tokenExpiration - DateTime.UtcNow).TotalSeconds,
                UserId = user.Id,
                UserName = user.Name
            });
        }

        private static List<Claim> CreateUserClaims(User user)
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

        private static string GetRequiredJwtSetting(IConfigurationSection jwtSettings, string key)
        {
            return jwtSettings[key]
                ?? throw new InvalidOperationException($"JWT setting '{key}' is not configured.");
        }
    }
}