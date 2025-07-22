using VocabMaster.Models;

namespace VocabMaster.Services.Interfaces
{
    public interface IDictionaryService
    {
        Task<DictionaryResponse> GetWordDefinitionAsync(string word); // Get word definition
        Task<DictionaryResponse> GetRandomWordAsync(); // Get random word
    }
}
