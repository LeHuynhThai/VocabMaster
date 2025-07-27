using System.Threading.Tasks;
using System.Collections.Generic;
using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for dictionary operations including word lookup and random word generation
    /// </summary>
    public interface IDictionaryService
    {
        /// <summary>
        /// Gets a random word and its definition
        /// </summary>
        /// <returns>Dictionary response with word details or null if not found</returns>
        Task<DictionaryResponseDto> GetRandomWord();
        
        /// <summary>
        /// Gets a random word, optionally trying to exclude words already learned by the user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Dictionary response with word details or null if not found</returns>
        Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId);
        
        /// <summary>
        /// Gets the definition of a word from the dictionary API
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Dictionary response with word details or null if not found</returns>
        Task<DictionaryResponseDto> GetWordDefinition(string word);

        /// <summary>
        /// Gets the definition of a word from the local database if available, otherwise from the API
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Dictionary response with word details or null if not found</returns>
        Task<DictionaryResponseDto> GetWordDefinitionFromCache(string word);
        
        /// <summary>
        /// Caches the definition of a word in the local database
        /// </summary>
        /// <param name="word">The word to cache</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> CacheWordDefinition(string word);
        
        /// <summary>
        /// Caches the definitions of all words in the vocabulary repository
        /// </summary>
        /// <returns>Number of words successfully cached</returns>
        Task<int> CacheAllVocabularyDefinitions();
    }
}
