using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Models;

namespace VocabMaster.Services.Interfaces
{
    public interface IVocabularyService
    {
        Task<bool> MarkWordAsLearnedAsync(string userId, string word, string note = null);
        Task<string> GetRandomUnlearnedWordAsync(string userId, List<string> allWords);
        Task<List<LearnedVocabulary>> GetUserLearnedVocabulariesAsync(string userId);
        Task<bool> IsWordLearnedAsync(string userId, string word);
        Task<int> GetUserProgressAsync(string userId);
    }
}

