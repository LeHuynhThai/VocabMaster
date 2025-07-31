using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services
{
    public interface IDictionaryService
    {
        Task<DictionaryResponseDto> GetRandomWord();

        Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId);

        Task<DictionaryResponseDto> GetWordDefinition(string word);

        Task<DictionaryResponseDto> GetWordDefinitionFromCache(string word);

        Task<bool> CacheWordDefinition(string word);
        
        Task<int> CacheAllVocabularyDefinitions();
    }
}
