using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Quiz
{
    public interface IQuizAnswerService
    {
        Task<QuizResultDto> CheckAnswer(int questionId, string answer);
        Task<QuizResultDto> CheckAnswerAndMarkCompleted(int questionId, string answer, int userId);
    }
} 