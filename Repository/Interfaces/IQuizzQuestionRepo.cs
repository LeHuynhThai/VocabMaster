using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IQuizzQuestionRepo
    {
        Task<List<int>> GetCompletedQuestionIds(int userId);
        Task<QuizQuestion?> GetRandomUncompletedQuestion(int userId);
    }
}
