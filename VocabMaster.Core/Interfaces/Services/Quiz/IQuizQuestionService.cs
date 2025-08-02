using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services.Quiz
{
    public interface IQuizQuestionService
    {
        Task<QuizQuestionDto> GetRandomQuestion();
        Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId);
        Task<int> CountTotalQuestions();
    }
} 