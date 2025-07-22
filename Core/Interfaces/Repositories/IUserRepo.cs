using VocabMaster.Core.Entities;

public interface IUserRepo
{
    Task<User> GetByName(string name); // Get user by name
    Task<bool> IsNameExist(string name); // Check if user name exists
    Task Add(User user); // Add user
    Task<User>  ValidateUser(string name, string password); // Validate user
}
