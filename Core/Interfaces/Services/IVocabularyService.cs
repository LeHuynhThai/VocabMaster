using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;
using VocabMaster.Services;

namespace VocabMaster.Core.Interfaces.Services
{
    public interface IVocabularyService
    {
        // mark a word as learned for a specific user
        Task<VocabularyService.MarkWordResult> MarkWordAsLearned(int userId, string word);

        // Gets all learned vocabularies for a specific user
        Task<List<LearnedVocabulary>> GetUserLearnedVocabularies(int userId);
        
        // Remove a learned word for a specific user
        Task<bool> RemoveLearnedWord(int userId, string word);
    }
}

