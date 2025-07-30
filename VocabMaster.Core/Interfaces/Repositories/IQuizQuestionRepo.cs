using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface for quiz question repository operations
    /// </summary>
    public interface IQuizQuestionRepo
    {
        /// <summary>
        /// Gets a random quiz question
        /// </summary>
        /// <returns>A random quiz question</returns>
        Task<QuizQuestion> GetRandomQuizQuestion();
        
        /// <summary>
        /// Gets a quiz question by id
        /// </summary>
        /// <param name="id">The quiz question id</param>
        /// <returns>The quiz question if found, null otherwise</returns>
        Task<QuizQuestion> GetQuizQuestionById(int id);
        
        /// <summary>
        /// Creates a new quiz question
        /// </summary>
        /// <param name="quizQuestion">The quiz question to create</param>
        /// <returns>The created quiz question</returns>
        Task<QuizQuestion> CreateQuizQuestion(QuizQuestion quizQuestion);
        
        /// <summary>
        /// Checks if there are any quiz questions
        /// </summary>
        /// <returns>True if there are quiz questions, false otherwise</returns>
        Task<bool> AnyQuizQuestions();
    }
} 