using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Dictionary
{
    public interface IDictionaryApiService
    {
        Task<DictionaryResponseDto> GetWordDefinitionFromApi(string word);
    }
}
