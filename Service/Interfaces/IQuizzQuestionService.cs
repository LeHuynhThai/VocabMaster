using Repository.Entities;

namespace Service.Interfaces
{
    public interface IQuizzQuestionService
    {
        Task<QuizQuestion?> GetRandomUncompletedQuestion(int userId);
    }
}
