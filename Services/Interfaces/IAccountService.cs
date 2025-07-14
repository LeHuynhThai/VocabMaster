using VocabMaster.Entities;

namespace VocabMaster.Services.Interfaces;

public interface IAccountService
{
    Task<User> LoginAsync(string name, string password); // Login
    Task<bool> RegisterAsync(User user); // Register
    string HashPassword(string password); // Hash password
    Task LogoutAsync(); // Logout
}
