using VocabMaster.Core.Entities;

namespace VocabMaster.Repositories.Interfaces
{
    public interface IVocabularyRepository
    {
        Task<Vocabulary> GetRandomAsync(); // Get random vocabulary
        Task<int> CountAsync(); // Get total count of vocabularies
    }
}