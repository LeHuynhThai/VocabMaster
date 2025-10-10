using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.Core.Interfaces.Services.Dictionary
{
    public interface IDictionaryLookupService
    {
        Task<DictionaryResponseDto> GetWordDefinition(string word);
        Task<DictionaryResponseDto> GetWordDefinitionFromDatabase(string word);
    }
}