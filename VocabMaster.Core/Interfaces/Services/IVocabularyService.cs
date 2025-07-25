using System.Collections.Generic;
using System.Threading.Tasks;
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
        // mark a word as learned for a specific user
        Task<MarkWordResult> MarkWordAsLearned(int userId, string word);

        // Gets all learned vocabularies for a specific user
        Task<List<LearnedWord>> GetUserLearnedVocabularies(int userId);
        
        // Remove a learned word for a specific user by word
        Task<bool> RemoveLearnedWord(int userId, string word);

        // Remove a learned word for a specific user by id
        Task<bool> RemoveLearnedWordById(int userId, int wordId);
    }
}

