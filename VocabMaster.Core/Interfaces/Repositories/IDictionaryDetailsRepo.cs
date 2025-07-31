using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface IDictionaryDetailsRepo
    {
        Task<DictionaryDetails> GetByWord(string word);

        Task<DictionaryDetails> AddOrUpdate(DictionaryDetails details);

        Task<List<DictionaryDetails>> GetAll();

        Task<bool> Exists(string word);
    }
}