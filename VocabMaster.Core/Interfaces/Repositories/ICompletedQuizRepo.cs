using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface for completed quiz repository operations
    /// </summary>
    public interface ICompletedQuizRepo
    {
        /// <summary>
        /// Gets all completed quiz questions for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of completed quiz questions</returns>
        Task<List<CompletedQuiz>> GetByUserId(int userId);

        /// <summary>
        /// Gets IDs of all completed quiz questions for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of completed quiz question IDs</returns>
        Task<List<int>> GetCompletedQuizQuestionIdsByUserId(int userId);

        /// <summary>
        /// Checks if a quiz question has been completed by a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="quizQuestionId">ID of the quiz question</param>
        /// <returns>True if completed, false otherwise</returns>
        Task<bool> IsQuizQuestionCompletedByUser(int userId, int quizQuestionId);

        /// <summary>
        /// Marks a quiz question as completed by a user
        /// </summary>
        /// <param name="completedQuiz">Completed quiz information</param>
        /// <returns>The created completed quiz record</returns>
        Task<CompletedQuiz> MarkAsCompleted(CompletedQuiz completedQuiz);
    }
}