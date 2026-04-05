using VocabMaster.Domain.Entities;

namespace VocabMaster.Domain.Interfaces
{
    public interface IQuizzStatRepo
    {
        Task<int> GetTotalQuestions();
        Task<List<CompletedQuiz>> GetCompletedQuizzes(int userId);
    }
}
