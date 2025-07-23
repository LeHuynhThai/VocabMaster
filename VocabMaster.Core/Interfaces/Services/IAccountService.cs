using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services;

public interface IAccountService
{
    Task<User> Login(string name, string password); // Login
    Task<bool> Register(User user); // Register
    string HashPassword(string password); // Hash password
    Task Logout(); // Logout
}
