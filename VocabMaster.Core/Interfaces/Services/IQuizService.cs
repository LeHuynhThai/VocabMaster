using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services
{
    public interface IQuizService
    {
        Task<QuizQuestionDto> GetRandomQuestion();

        Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId);

        Task<QuizResultDto> CheckAnswer(int questionId, string answer);

        Task<QuizResultDto> CheckAnswerAndMarkCompleted(int questionId, string answer, int userId);

        Task<List<CompletedQuizDto>> GetCompletedQuizzes(int userId);

        Task<QuizStatsDto> GetQuizStatistics(int userId);

        Task<int> CountTotalQuestions();
    }
}