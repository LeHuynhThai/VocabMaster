using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepo _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserRepo userRepository,
            IHttpContextAccessor httpContextAccessor,
            ITokenService tokenService,
            IPasswordService passwordService,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TokenResponseDto> Login(string name, string password)
        {
            var user = await _userRepository.ValidateUser(name, password);
            return user != null ? await _tokenService.GenerateJwtToken(user) : null;
        }

        public async Task<bool> Register(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            if (await _userRepository.IsNameExist(user.Name))
                return false;

            user.Password = _passwordService.HashPassword(user.Password);
            await _userRepository.Add(user);
            return true;
        }

        public async Task Logout()
        {
            // JWT not need to logout on server
            await Task.CompletedTask;
        }

        public async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null || 
                !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userId = _tokenService.FindUserIdFromClaims(_httpContextAccessor.HttpContext.User);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return null;
            }

            return await _userRepository.GetById(id);
        }
    }
} 