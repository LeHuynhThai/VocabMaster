using VocabMaster.Models;

namespace VocabMaster.Services.Interfaces
{
    public interface IDictionaryService
    {
        Task<DictionaryResponse> GetRandomWordAsync();
    }
}
