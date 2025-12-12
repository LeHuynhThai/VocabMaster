using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Repository.Entities;
using Service.Interfaces;
using Repository.Interfaces;

namespace Service.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepo _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthenticationService(
            IUserRepo userRepository,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
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

        public async Task<Dictionary<string, object>?> AuthenticateGoogleUser(string accessToken)
        {
            try
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    Console.WriteLine("AccessToken is null/empty");
                    return null;
                }

                Console.WriteLine($"Starting Google authentication with token length: {accessToken.Length}");

                var googleUserInfo = await GetGoogleUserInfo(accessToken);
                if (googleUserInfo == null || !googleUserInfo.TryGetValue("email", out var emailObj) || emailObj == null || string.IsNullOrEmpty(emailObj.ToString()))
                {
                    Console.WriteLine("Failed to get valid Google user info or email is missing");
                    return null;
                }

                var email = emailObj.ToString();

                Console.WriteLine($"Google user info received: {googleUserInfo}");

                var existingUser = await _userRepository.GetByName(email);
                Console.WriteLine($"Existing user found: {existingUser != null}");

                if (existingUser == null)
                {
                    existingUser = await CreateGoogleUser(googleUserInfo);
                    if (existingUser == null) return null;
                }

                Console.WriteLine($"Generating JWT token for user ID: {existingUser.Id}");
                return await GenerateJwtToken(existingUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Dictionary<string, object>?> GetGoogleUserInfo(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("AccessToken is null or empty");
                return null;
            }

            try
            {
                Console.WriteLine($"Getting user info with token length: {accessToken.Length}");
                var httpClient = _httpClientFactory.CreateClient("GoogleApi");

                var userInfo = await TryGetGoogleUserInfo(
                    httpClient,
                    "https://www.googleapis.com/oauth2/v3/userinfo",
                    useAuthHeader: true,
                    accessToken);

                if (userInfo != null) return userInfo;

                return await TryGetGoogleUserInfo(
                    httpClient,
                    $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}",
                    useAuthHeader: false,
                    accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private async Task<Dictionary<string, object>?> TryGetGoogleUserInfo(HttpClient httpClient, string url, bool useAuthHeader, string accessToken)
        {
            try
            {
                if (useAuthHeader)
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Authorization = null;
                }

                Console.WriteLine($"Trying URL: {url}");
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to get user info. Status: {response.StatusCode}, Content: {content}");
                    return null;
                }

                var userInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                if (userInfo == null || !userInfo.TryGetValue("email", out var emailObj) || emailObj == null)
                {
                    Console.WriteLine("Failed to deserialize Google user info");
                    return null;
                }

                Console.WriteLine($"Successfully obtained user info for: {emailObj}");
                return userInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private async Task<User> CreateGoogleUser(Dictionary<string, object> googleUserInfo)
        {
            if (googleUserInfo == null) throw new ArgumentNullException(nameof(googleUserInfo));

            googleUserInfo.TryGetValue("email", out var emailObj);
            var email = emailObj?.ToString();
            if (string.IsNullOrWhiteSpace(email)) return null;

            var newUser = new User
            {
                Name = email,
                Password = HashPassword(Guid.NewGuid().ToString()),
                Role = UserRole.User
            };

            Console.WriteLine($"Creating new user with email: {newUser.Name}");
            await _userRepository.Add(newUser);
            return await _userRepository.GetByName(email);
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