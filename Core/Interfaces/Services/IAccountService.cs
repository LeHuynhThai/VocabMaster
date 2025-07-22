using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services;

public interface IAccountService
{
    Task<User> LoginAsync(string name, string password); // Login
    Task<bool> RegisterAsync(User user); // Register
    string HashPassword(string password); // Hash password
    Task LogoutAsync(); // Logout
}
