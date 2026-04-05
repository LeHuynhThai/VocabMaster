using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
// Removed System.Text.Json usage as Google OAuth helpers were removed
using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;

namespace VocabMaster.Infrastructure.Identity
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepo _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepo userRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Dictionary<string, object>?> Login(string name, string password)
        {
            var user = await _userRepository.ValidateUser(name, password);
            return user != null ? await GenerateJwtToken(user) : null;
        }

        public async Task<bool> Register(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (await _userRepository.IsNameExist(user.Name))
                return false;

            user.Password = HashPassword(user.Password);
            await _userRepository.Add(user);
            return true;
        }

        public async Task Logout()
        {
            await Task.CompletedTask;
        }

        public async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null ||
                !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userId = FindUserIdFromClaims(_httpContextAccessor.HttpContext.User);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return null;
            }

            return await _userRepository.GetById(id);
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash cannot be null or empty", nameof(hash));

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public async Task<Dictionary<string, object>> GenerateJwtToken(User user)
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

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["accessToken"] = new JwtSecurityTokenHandler().WriteToken(token),
                ["expiresIn"] = (int)(tokenExpiration - DateTime.UtcNow).TotalSeconds,
                ["userId"] = user.Id,
                ["userName"] = user.Name,
                ["role"] = user.Role.ToString()
            });
        }

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