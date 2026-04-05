using VocabMaster.Application.Models.Quiz;

namespace VocabMaster.Application.Interfaces
{
    public interface IQuizzStatService
    {
        Task<QuizStatsSummary> GetQuizStats(int userId);
        Task<List<CompletedQuizAnswerSummary>> GetCompletedAnswers(int userId);
    }
}