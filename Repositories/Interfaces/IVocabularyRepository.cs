using VocabMaster.Entities;

public interface IVocabularyRepository
{
    Task<Vocabulary> GetRandomAsync(); // Get random vocabulary
    Task<int> GetTotalCountAsync(); // Get total count of vocabularies
    Task<IEnumerable<Vocabulary>> GetAllAsync(); // Get all vocabularies
}