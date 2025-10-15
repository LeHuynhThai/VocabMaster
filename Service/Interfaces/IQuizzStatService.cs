using Repository.Entities;

namespace Service.Interfaces
{
    public interface IQuizzStatService
    {
        Task<int> GetTotalQuestions();
        Task<List<CompletedQuiz>> GetCompletedQuizzes(int userId);
    }
}