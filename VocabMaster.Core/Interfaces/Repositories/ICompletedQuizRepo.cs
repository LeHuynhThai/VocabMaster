using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    public interface ICompletedQuizRepo
    {
        Task<List<CompletedQuiz>> GetByUserId(int userId);

        Task<List<int>> GetCompletedQuizQuestionIdsByUserId(int userId);

        Task<bool> IsQuizQuestionCompletedByUser(int userId, int quizQuestionId);

        Task<CompletedQuiz> MarkAsCompleted(CompletedQuiz completedQuiz);
    }
}