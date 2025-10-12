using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IDictionaryDetailsRepo
    {
        Task<Vocabulary> GetByWord(string word);

        Task<Vocabulary> AddOrUpdate(Vocabulary details);

        Task<List<Vocabulary>> GetAll();

        Task<bool> Exists(string word);
    }
}