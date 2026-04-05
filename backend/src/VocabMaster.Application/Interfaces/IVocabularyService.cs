using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces
{
    public interface IVocabularyService
    {
        Task<Vocabulary?> GetRandomWord(int userId);
        Task<List<LearnedWord>> GetLearnedWords(int userId);
        Task<LearnedWord> AddLearnedWord(string word, int userId);
        Task<bool> RemoveLearnedWord(int learnedWordId, int userId);
        Task<Vocabulary?> GetVocabularyByWord(string word);
    }
}