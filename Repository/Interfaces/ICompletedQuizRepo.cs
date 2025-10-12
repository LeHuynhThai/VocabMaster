using Repository.Entities;

namespace Repository.Interfaces
{
    public interface ICompletedQuizRepo
    {
        Task<List<CompletedQuiz>> GetByUserId(int userId);

        Task<List<int>> GetCompletedQuizQuestionIdsByUserId(int userId);

        Task<bool> IsQuizQuestionCompletedByUser(int userId, int quizQuestionId);

        Task<CompletedQuiz> MarkAsCompleted(CompletedQuiz completedQuiz);
        
        Task<(List<CompletedQuiz> Items, int TotalCount)> GetPaginatedCorrectQuizzes(int userId, int pageNumber, int pageSize);
    }
}