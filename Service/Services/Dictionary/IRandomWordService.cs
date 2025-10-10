using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Dictionary
{
    public interface IRandomWordService
    {
        Task<DictionaryResponseDto> GetRandomWord();
        Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId);
    }
}