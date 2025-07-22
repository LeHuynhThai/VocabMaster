using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface ILearnedVocabularyRepository
    {
        Task<LearnedVocabulary> GetById(int id); // get a learned vocabulary by its ID
        Task<List<LearnedVocabulary>> GetByUserId(int userId); // get all learned vocabularies by user ID
        Task<bool> Add(LearnedVocabulary learnedVocabulary); // add a learned vocabulary
        Task<bool> Delete(int id); // delete a learned vocabulary by its ID
    }
} 