using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Entities;

namespace VocabMaster.Repositories.Interfaces
{
    public interface ILearnedVocabularyRepository
    {
        Task<LearnedVocabulary> GetByIdAsync(int id); // get a learned vocabulary by its ID
        Task<List<LearnedVocabulary>> GetByUserIdAsync(int userId); // get all learned vocabularies by user ID
        Task<bool> AddAsync(LearnedVocabulary learnedVocabulary); // add a learned vocabulary
        Task<bool> DeleteAsync(int id); // delete a learned vocabulary by its ID
    }
} 