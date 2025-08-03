using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Quiz
{
    public interface IQuizProgressService
    {
        Task<List<CompletedQuizDto>> GetCompletedQuizzes(int userId);
        Task<QuizStatsDto> GetQuizStatistics(int userId);
    }
}