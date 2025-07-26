using System.Threading.Tasks;
using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for dictionary service operations
    /// </summary>
    public interface IDictionaryService
    {
        /// <summary>
        /// Gets the definition of a word from the dictionary
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Dictionary response with word details or null if not found</returns>
        Task<DictionaryResponseDto> GetWordDefinition(string word);
        
        /// <summary>
        /// Gets a random word and its definition
        /// </summary>
        /// <returns>Dictionary response with word details or null if not found</returns>
        Task<DictionaryResponseDto> GetRandomWord();
        
        /// <summary>
        /// Gets a random word excluding words already learned by the user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Dictionary response with word details or null if not found or all words have been learned</returns>
        Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId);
        
        /// <summary>
        /// Gets the definition of a word and translates relevant parts to Vietnamese
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Dictionary response with translated details or null if not found</returns>
        Task<DictionaryResponseDto> GetWordDefinitionWithTranslation(string word);
    }
}
