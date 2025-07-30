using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for dictionary details operations
    /// </summary>
    public interface IDictionaryDetailsRepo
    {
        /// <summary>
        /// Gets dictionary details by word
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Dictionary details or null if not found</returns>
        Task<DictionaryDetails> GetByWord(string word);

        /// <summary>
        /// Adds or updates dictionary details
        /// </summary>
        /// <param name="details">The dictionary details to save</param>
        /// <returns>The saved dictionary details</returns>
        Task<DictionaryDetails> AddOrUpdate(DictionaryDetails details);

        /// <summary>
        /// Gets all dictionary details
        /// </summary>
        /// <returns>List of all dictionary details</returns>
        Task<List<DictionaryDetails>> GetAll();

        /// <summary>
        /// Checks if dictionary details exist for a word
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <returns>True if details exist, false otherwise</returns>
        Task<bool> Exists(string word);
    }
}