using Repository.Entities;
namespace Service.Interfaces
{
    public interface IVocabularyService
    {
        Task<Vocabulary> GetRandomWord(int userId);
        Task<List<LearnedWord>> GetLearnedWords(int userId);
    }
}