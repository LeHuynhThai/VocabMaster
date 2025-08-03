using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services.Dictionary
{
    public interface IDictionaryCacheService
    {
        Task<int> CacheAllVocabularyDefinitions();
    }
}