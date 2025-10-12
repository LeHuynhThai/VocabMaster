using Repository.DTOs;

namespace Service.Interfaces
{
    public interface IDictionaryCacheService
    {
        Task<int> CacheAllVocabularyDefinitions();
        Task<DictionaryResponseDto> GetWordDefinition(string word);
        Task<DictionaryResponseDto> GetWordDefinitionFromDatabase(string word);
        Task<DictionaryResponseDto> GetRandomWord();
        Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId);
    }
}