using VocabMaster.Entities;

namespace VocabMaster.Services.Interfaces;

public interface IAccountService
{
    Task<User> LoginAsync(string name, string password); // Login
    Task<bool> RegisterAsync(User user); // Register
    Task LogoutAsync(); // Logout
}
