using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepo _userRepository; // User repository
    private readonly IHttpContextAccessor _httpContextAccessor; // Http context accessor

    // Constructor
    public AccountService(IUserRepo userRepository,
                         IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    // Login
    public async Task<User> Login(string name, string password)
    {
        var user = await _userRepository.ValidateUser(name, password); // ValidateUser method in UserRepo
        if (user != null) // If user exists
        {
            var userClaims = CreateUserClaims(user); // Create user claims
            var userIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme); // Create user identity
            var cookieProperties = CreateAuthenticationProperties(); // Create authentication properties

            // Sign in user
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(userIdentity),
                cookieProperties);
        }

        return user;
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
        if (_httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Session.Clear();
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}


