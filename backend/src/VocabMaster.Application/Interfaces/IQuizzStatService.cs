using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces
{
    public interface IQuizzStatService
    {
        Task<int> GetTotalQuestions();
        Task<List<CompletedQuiz>> GetCompletedQuizzes(int userId);
    }
}