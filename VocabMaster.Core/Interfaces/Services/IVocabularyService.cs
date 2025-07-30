using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services
{
    public class MarkWordResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public LearnedWord Data { get; set; }
    }

    /// <summary>
    /// Interface for vocabulary service operations
    /// </summary>
    public interface IVocabularyService
    {
        /// <summary>
        /// Adds a word to the user's learned words list
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="word">Word to add to learned list</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> AddLearnedWord(int userId, string word);

        /// <summary>
        /// Checks if a word is in the user's learned words list
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="word">Word to check</param>
        /// <returns>True if the word is learned, false otherwise</returns>
        Task<bool> IsWordLearned(int userId, string word);

        /// <summary>
        /// Marks a word as learned for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="word">Word to mark as learned</param>
        /// <returns>Result of the operation with success status and data</returns>
        Task<MarkWordResult> MarkWordAsLearned(int userId, string word);

        /// <summary>
        /// Gets all learned words for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of learned words</returns>
        Task<List<LearnedWord>> GetUserLearnedVocabularies(int userId);

        /// <summary>
        /// Removes a learned word for a specific user by ID
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="wordId">ID of the learned word to remove</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RemoveLearnedWordById(int userId, int wordId);

        /// <summary>
        /// Gets a learned word by its ID for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="wordId">ID of the learned word</param>
        /// <returns>The learned word or null if not found</returns>
        Task<LearnedWord> GetLearnedWordById(int userId, int wordId);
    }
}

