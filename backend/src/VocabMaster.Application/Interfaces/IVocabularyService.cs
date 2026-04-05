using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces
{
    public interface IVocabularyService
    {
        Task<Vocabulary?> GetRandomWord(int userId);
        Task<List<LearnedWord>> GetLearnedWords(int userId);
        Task<LearnedWord> AddLearnedWord(LearnedWord learnedWord);
        Task<bool> RemoveLearnedWord(int learnedWordId);
        Task<Vocabulary?> GetVocabularyByWord(string word);
    }
}