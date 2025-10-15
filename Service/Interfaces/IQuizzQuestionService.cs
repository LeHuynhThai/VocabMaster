using Repository.Entities;

namespace Service.Interfaces
{
    public interface IQuizzQuestionService
    {
        Task<QuizQuestion?> GetRandomUncompletedQuestion(int userId);
        Task<bool> SubmitQuizAnswer(int userId, int quizQuestionId, string selectedAnswer);
    }
}
