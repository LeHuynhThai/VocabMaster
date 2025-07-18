using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Entities;

namespace VocabMaster.Services.Interfaces
{
    /// <summary>
    /// Service interface for vocabulary management
    /// </summary>
    public interface IVocabularyService
    {
        /// <summary>
        /// Marks a word as learned for a specific user
        /// </summary>
        Task<bool> MarkWordAsLearnedAsync(int userId, string word);

        /// <summary>
        /// Gets all learned vocabularies for a specific user
        /// </summary>
        Task<List<LearnedVocabulary>> GetUserLearnedVocabulariesAsync(int userId);
    }
}

