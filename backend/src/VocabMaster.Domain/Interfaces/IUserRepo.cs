using VocabMaster.Domain.Entities;

namespace VocabMaster.Domain.Interfaces
{
    public interface IUserRepo
    {
        Task<User?> GetByName(string name);
        Task<User?> GetById(int id);
        Task<bool> IsNameExist(string name);
        Task Add(User user);
    }
}
