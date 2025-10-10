using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Quiz
{
    public interface IQuizProgressService
    {
        Task<List<CompletedQuizDto>> GetCompletedQuizzes(int userId);
        Task<List<CompletedQuizDto>> GetCompleteQuizz(int userId);
        Task<QuizStatsDto> GetQuizStatistics(int userId);
        Task<(List<CompletedQuizDto> Items, int TotalCount, int TotalPages)> GetPaginatedCorrectQuizzes(int userId, int pageNumber, int pageSize);
    }
}