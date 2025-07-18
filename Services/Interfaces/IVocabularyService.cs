using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Entities;

namespace VocabMaster.Services.Interfaces
{
    public interface IVocabularyService
    {
        // mark a word as learned for a specific user
        Task<bool> MarkWordAsLearnedAsync(int userId, string word);

        // Gets all learned vocabularies for a specific user
        Task<List<LearnedVocabulary>> GetUserLearnedVocabulariesAsync(int userId);
    }
}

