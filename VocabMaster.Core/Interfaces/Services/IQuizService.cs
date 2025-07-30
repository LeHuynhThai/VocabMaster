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
    }
} 