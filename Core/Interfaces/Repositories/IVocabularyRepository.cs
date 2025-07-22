using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface IVocabularyRepository
    {
        Task<Vocabulary> GetRandomAsync(); // Get random vocabulary
        Task<int> CountAsync(); // Get total count of vocabularies
        
        // Get random vocabulary excluding learned words
        Task<Vocabulary> GetRandomExcludeLearnedAsync(List<string> learnedWords);
    }
}