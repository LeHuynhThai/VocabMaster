using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services.Dictionary
{
    public interface IDictionaryCacheService
    {
        Task<DictionaryDetails> CacheDefinition(DictionaryResponseDto definition);
        Task<bool> CacheWordDefinition(string word);
        Task<int> CacheAllVocabularyDefinitions();
    }
}