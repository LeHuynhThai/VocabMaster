using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services
{
    public class MarkWordResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public LearnedWord Data { get; set; }
    }

    public interface IVocabularyService
    {
        Task<bool> AddLearnedWord(int userId, string word);

        Task<bool> IsWordLearned(int userId, string word);

        Task<MarkWordResult> MarkWordAsLearned(int userId, string word);

        Task<List<LearnedWord>> GetUserLearnedVocabularies(int userId);

        Task<bool> RemoveLearnedWordById(int userId, int wordId);

        Task<LearnedWord> GetLearnedWordById(int userId, int wordId);
    }
}

