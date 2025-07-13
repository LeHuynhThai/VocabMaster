using VocabMaster.Entities;

public interface IUserRepository
{
    Task<User> GetByNameAsync(string name); // Get user by name
    Task<bool> IsNameExistsAsync(string name); // Check if user name exists
    Task AddAsync(User user); // Add user
    Task<User> LoginAsync(string name, string password); // Login
}
