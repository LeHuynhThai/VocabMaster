using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services
{
    public interface IDictionaryService
    {
        Task<DictionaryResponseDto> GetWordDefinitionAsync(string word); // Get word definition
        Task<DictionaryResponseDto> GetRandomWordAsync(); // Get random word
    }
}
