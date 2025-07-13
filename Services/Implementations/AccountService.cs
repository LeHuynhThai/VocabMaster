using VocabMaster.Entities;
using VocabMaster.Repositories;
using VocabMaster.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace VocabMaster.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository; // User repository
    private readonly IHttpContextAccessor _httpContextAccessor; // Http context accessor

    // Constructor
    public AccountService(IUserRepository userRepository, 
                         IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    // Login
    public async Task<User> LoginAsync(string name, string password)
    {
        var user = await _userRepository.LoginAsync(name, password); // Login
        if (user != null) // If user exists
        {
            var claims = new List<Claim> // Claims
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // Claims identity
            var authProperties = new AuthenticationProperties // Authentication properties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        return user;
    }

    public async Task<bool> RegisterAsync(User user)
    {
        if (await _userRepository.IsNameExistsAsync(user.Name))
            return false;

        await _userRepository.AddAsync(user);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}


