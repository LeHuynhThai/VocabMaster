using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface ILearnedWordRepo
    {
        Task<LearnedWord> GetById(int id);

        Task<List<LearnedWord>> GetByUserId(int userId);

        Task<bool> Add(LearnedWord learnedWord);

        Task<bool> Delete(int id);

        Task<bool> RemoveByWord(int userId, string word);
    }
}