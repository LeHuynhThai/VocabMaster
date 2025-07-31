using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services
{
    public interface IVocabularyService
    {
        Task<MarkWordResultDto> MarkWordAsLearned(int userId, string word);

        Task<bool> AddLearnedWord(int userId, string word);

        Task<bool> IsWordLearned(int userId, string word);

        Task<List<LearnedWord>> GetUserLearnedVocabularies(int userId);

        Task<bool> RemoveLearnedWordById(int userId, int wordId);

        Task<LearnedWord> GetLearnedWordById(int userId, int wordId);
    }
}

