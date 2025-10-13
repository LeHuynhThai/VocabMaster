using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IUserRepo
    {
        Task<User> GetByName(string name);
        Task<User> GetById(int id);
        Task<bool> IsNameExist(string name);
        Task Add(User user);
        Task<User> ValidateUser(string name, string password);
    }
}
