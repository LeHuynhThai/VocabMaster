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
        /// Gets a random quiz question excluding specific IDs
        /// </summary>
        /// <param name="excludeIds">IDs to exclude</param>
        /// <returns>A random quiz question not in the excluded IDs</returns>
        Task<QuizQuestion> GetRandomUnansweredQuizQuestion(List<int> excludeIds);
        
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

        /// <summary>
        /// Gets the total number of quiz questions
        /// </summary>
        /// <returns>The total number of quiz questions</returns>
        Task<int> CountQuizQuestions();
    }
} 