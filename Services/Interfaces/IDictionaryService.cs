using VocabMaster.Models;

namespace VocabMaster.Services.Interfaces
{
    public interface IDictionaryService
    {
        Task<DictionaryResponse> GetWordDefinitionAsync(string word);
        Task<DictionaryResponse> GetRandomWordAsync();
    }
}
