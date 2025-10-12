using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IQuizQuestionRepo
    {
        Task<QuizQuestion> GetRandomQuizQuestion();

        Task<QuizQuestion> GetRandomUnansweredQuizQuestion(List<int> excludeIds);

        Task<QuizQuestion> GetQuizQuestionById(int id);

        Task<bool> AnyQuizQuestions();

        Task<int> CountQuizQuestions();
    }
}