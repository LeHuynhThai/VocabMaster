using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services
{
    public interface IDictionaryService
    {
        Task<DictionaryResponseDto> GetWordDefinition(string word); // Get word definition
        Task<DictionaryResponseDto> GetRandomWord(); // Get random word
        
        // Get random word excluding learned words
        Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId);
    }
}
