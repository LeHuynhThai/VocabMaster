using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services.Dictionary
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