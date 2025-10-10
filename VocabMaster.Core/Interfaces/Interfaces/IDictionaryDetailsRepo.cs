using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface IDictionaryDetailsRepo
    {
        Task<Vocabulary> GetByWord(string word);

        Task<Vocabulary> AddOrUpdate(Vocabulary details);

        Task<List<Vocabulary>> GetAll();

        Task<bool> Exists(string word);
    }
}