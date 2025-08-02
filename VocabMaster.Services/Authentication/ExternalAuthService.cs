using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services.Authentication
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly IUserRepo _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExternalAuthService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExternalAuthService(
            IUserRepo userRepository,
            ITokenService tokenService,
            IPasswordService passwordService,
            IHttpClientFactory httpClientFactory,
            ILogger<ExternalAuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<TokenResponseDto> AuthenticateGoogleUser(GoogleAuthDto googleAuth)
        {
            try
            {
                if (googleAuth == null || string.IsNullOrEmpty(googleAuth.AccessToken))
                {
                    _logger.LogError("GoogleAuthDto or AccessToken is null/empty");
                    return null;
                }

                _logger.LogInformation($"Starting Google authentication with token length: {googleAuth.AccessToken.Length}");

                var googleUserInfo = await GetGoogleUserInfo(googleAuth.AccessToken);
                if (googleUserInfo == null || string.IsNullOrEmpty(googleUserInfo.Email))
                {
                    _logger.LogError("Failed to get valid Google user info or email is missing");
                    return null;
                }

                _logger.LogInformation($"Google user info received: {googleUserInfo}");

                var existingUser = await _userRepository.GetByName(googleUserInfo.Email);
                _logger.LogInformation($"Existing user found: {existingUser != null}");

                if (existingUser == null)
                {
                    existingUser = await CreateGoogleUser(googleUserInfo);
                    if (existingUser == null) return null;
                }

                _logger.LogInformation($"Generating JWT token for user ID: {existingUser.Id}");
                return await _tokenService.GenerateJwtToken(existingUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating Google user");
                return null;
            }
        }

        public async Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("AccessToken is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation($"Getting user info with token length: {accessToken.Length}");
                var httpClient = _httpClientFactory.CreateClient("GoogleApi");
                
                // Try method 1: Using Authorization header
                var userInfo = await TryGetGoogleUserInfo(
                    httpClient, 
                    "https://www.googleapis.com/oauth2/v3/userinfo",
                    useAuthHeader: true, 
                    accessToken);
                    
                if (userInfo != null) return userInfo;
                
                // Try method 2: Using query parameter
                return await TryGetGoogleUserInfo(
                    httpClient,
                    $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}",
                    useAuthHeader: false,
                    accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google user info");
                return null;
            }
        }
        
        private async Task<GoogleUserInfoDto> TryGetGoogleUserInfo(HttpClient httpClient, string url, bool useAuthHeader, string accessToken)
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
                
                _logger.LogInformation($"Trying URL: {url}");
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"Response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get user info. Status: {response.StatusCode}, Content: {content}");
                    return null;
                }
                
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfoDto>(content, _jsonOptions);
                if (userInfo == null)
                {
                    _logger.LogError("Failed to deserialize Google user info");
                    return null;
                }
                
                _logger.LogInformation($"Successfully obtained user info for: {userInfo.Email}");
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in TryGetGoogleUserInfo ({(useAuthHeader ? "auth header" : "query param")})");
                return null;
            }
        }

        private async Task<User> CreateGoogleUser(GoogleUserInfoDto googleUserInfo)
        {
            var newUser = new User
            {
                Name = googleUserInfo.Email,
                Password = _passwordService.HashPassword(Guid.NewGuid().ToString()),
                Role = UserRole.User
            };

            _logger.LogInformation($"Creating new user with email: {newUser.Name}");
            await _userRepository.Add(newUser);
            return await _userRepository.GetByName(googleUserInfo.Email);
        }
    }
} 