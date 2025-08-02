using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Dictionary
{
    public interface IDictionaryLookupService
    {
        Task<DictionaryResponseDto> GetWordDefinition(string word);
        Task<DictionaryResponseDto> GetWordDefinitionFromCache(string word);
    }
}