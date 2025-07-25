using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface for learned word repository operations
    /// </summary>
    public interface ILearnedWordRepo
    {
        /// <summary>
        /// Gets a learned word by its ID
        /// </summary>
        /// <param name="id">ID of the learned word</param>
        /// <returns>Learned word or null if not found</returns>
        Task<LearnedWord> GetById(int id);
        
        /// <summary>
        /// Gets all learned words for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of learned words</returns>
        Task<List<LearnedWord>> GetByUserId(int userId);
        
        /// <summary>
        /// Adds a new learned word
        /// </summary>
        /// <param name="learnedWord">Learned word to add</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> Add(LearnedWord learnedWord);
        
        /// <summary>
        /// Deletes a learned word by its ID
        /// </summary>
        /// <param name="id">ID of the learned word to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> Delete(int id);
        
        /// <summary>
        /// Removes a learned word for a specific user by word text
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="word">Word to remove</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RemoveByWord(int userId, string word);
    }
} 