using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface IVocabularyRepo
    {
        Task<Vocabulary> GetRandom();

        Task<int> Count();

        Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords);

        Task<List<Vocabulary>> GetAll();

        Task<bool> Update(Vocabulary vocabulary); // Update a vocabulary, use for crawl Vietnamese from api
    }
}