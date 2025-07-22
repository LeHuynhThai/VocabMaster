using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface IVocabularyRepository
    {
        Task<Vocabulary> GetRandom(); // Get random vocabulary
        Task<int> Count(); // Get total count of vocabularies
        
        // Get random vocabulary excluding learned words
        Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords);
    }
}