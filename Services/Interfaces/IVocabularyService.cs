using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Entities;
using VocabMaster.Services.Implementations;

namespace VocabMaster.Services.Interfaces
{
    public interface IVocabularyService
    {
        // mark a word as learned for a specific user
        Task<VocabularyService.MarkWordResult> MarkWordAsLearnedAsync(int userId, string word);

        // Gets all learned vocabularies for a specific user
        Task<List<LearnedVocabulary>> GetUserLearnedVocabulariesAsync(int userId);
        
        // Remove a learned word for a specific user
        Task<bool> RemoveLearnedWordAsync(int userId, string word);
    }
}

