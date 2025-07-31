using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
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