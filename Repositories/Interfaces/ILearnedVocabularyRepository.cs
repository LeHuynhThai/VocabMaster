using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Entities;

namespace VocabMaster.Repositories.Interfaces
{
    public interface ILearnedVocabularyRepository
    {
        Task<LearnedVocabulary> GetByIdAsync(int id);
        Task<List<LearnedVocabulary>> GetByUserIdAsync(int userId);
        Task<bool> AddAsync(LearnedVocabulary learnedVocabulary);
        Task<bool> DeleteAsync(int id);
    }
} 