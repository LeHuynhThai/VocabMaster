using VocabMaster.Domain.Entities;

namespace VocabMaster.Domain.Interfaces
{
    public interface IQuizzQuestionRepo
    {
        Task<QuizQuestion?> GetRandomUncompletedQuestion(int userId);
        Task<QuizQuestion?> GetQuestionById(int questionId);
        Task<CompletedQuiz?> GetCompletedQuiz(int userId, int quizQuestionId);
        Task<CompletedQuiz> SaveCompletedQuiz(int userId, int quizQuestionId, bool wasCorrect);
    }
}
