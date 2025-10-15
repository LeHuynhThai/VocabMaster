using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IQuizzStatRepo
    {
        Task<int> GetTotalQuestions();
        Task<List<CompletedQuiz>> GetCompletedQuizzes(int userId);
    }
}
