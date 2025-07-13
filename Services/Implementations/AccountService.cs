using VocabMaster.Entities;
using VocabMaster.Repositories;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository; // User repository
    private readonly IHttpContextAccessor _httpContextAccessor; // HttpContextAccessor

    // Constructor
    public AccountService(IUserRepository userRepository, 
                         IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository; // User repository
        _httpContextAccessor = httpContextAccessor; // HttpContextAccessor
    }

    // Login
    public async Task<bool> LoginAsync(string name)
    {
        var user = await _userRepository.GetByNameAsync(name); // Get user by name
        if (user != null) // If user exists
        {
            _httpContextAccessor.HttpContext.Session.SetString("Name", name); // Set user name in session
            return true;
        }
        return false;
    }

    // Register
    public async Task<bool> RegisterAsync(User user)
    {
        if (await _userRepository.IsNameExistsAsync(user.Name)) // If user name exists
            return false;

        await _userRepository.AddAsync(user); // Add user
        return true;
    }

    // Logout
    public void Logout()
    {
        _httpContextAccessor.HttpContext.Session.Clear(); // Clear session
    }
}


