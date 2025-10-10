using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Quiz
{
    public interface IQuizAnswerService
    {
        Task<QuizResultDto> CheckAnswer(int questionId, string answer);
        Task<QuizResultDto> CheckAnswerAndMarkCompleted(int questionId, string answer, int userId);
        Task<List<CompletedQuizDto>> GetCompletedQuizzes(int userId);
        Task<List<CompletedQuizDto>> GetCompleteQuizz(int userId);
        Task<QuizStatsDto> GetQuizStatistics(int userId);
        Task<(List<CompletedQuizDto> Items, int TotalCount, int TotalPages)> GetPaginatedCorrectQuizzes(int userId, int pageNumber, int pageSize);
        Task<QuizQuestionDto> GetRandomQuestion();
        Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId);
    }

}