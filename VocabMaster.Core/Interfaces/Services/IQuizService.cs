using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for quiz service operations
    /// </summary>
    public interface IQuizService
    {
        /// <summary>
        /// Gets a random quiz question
        /// </summary>
        /// <returns>A random quiz question DTO</returns>
        Task<QuizQuestionDto> GetRandomQuestion();

        /// <summary>
        /// Gets a random quiz question that hasn't been completed by the user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>A random uncompleted quiz question DTO</returns>
        Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId);
        
        /// <summary>
        /// Creates a quiz question with random answers from vocabulary
        /// </summary>
        /// <returns>The created quiz question</returns>
        Task<QuizQuestion> CreateQuizQuestionFromVocabulary();
        
        /// <summary>
        /// Verifies an answer to a quiz question
        /// </summary>
        /// <param name="questionId">The ID of the question</param>
        /// <param name="answer">The selected answer</param>
        /// <returns>Quiz result with information about correct/incorrect answer</returns>
        Task<QuizResultDto> CheckAnswer(int questionId, string answer);

        /// <summary>
        /// Verifies an answer to a quiz question and marks it as completed if correct
        /// </summary>
        /// <param name="questionId">The ID of the question</param>
        /// <param name="answer">The selected answer</param>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Quiz result with information about correct/incorrect answer</returns>
        Task<QuizResultDto> CheckAnswerAndMarkCompleted(int questionId, string answer, int userId);

        /// <summary>
        /// Gets all completed quiz questions for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of completed quiz questions</returns>
        Task<List<CompletedQuizDto>> GetCompletedQuizzes(int userId);

        /// <summary>
        /// Gets statistics about a user's quiz progress
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Quiz statistics</returns>
        Task<QuizStatsDto> GetQuizStatistics(int userId);

        /// <summary>
        /// Gets the total number of quiz questions in the system
        /// </summary>
        /// <returns>Total number of questions</returns>
        Task<int> CountTotalQuestions();
    }

    /// <summary>
    /// DTO for quiz statistics
    /// </summary>
    public class QuizStatsDto
    {
        public int TotalQuestions { get; set; }
        public int CompletedQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double CompletionPercentage { get; set; }
        public double CorrectPercentage { get; set; }
    }
} 