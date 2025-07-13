using VocabMaster.Entities;

namespace VocabMaster.Services.Interfaces;

public interface IAccountService
{
    Task<bool> LoginAsync(string name); // Login
    Task<bool> RegisterAsync(User user); // Register
    void Logout(); // Logout
}
