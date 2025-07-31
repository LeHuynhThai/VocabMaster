using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    /// <summary>
    /// Repository implementation for completed quiz operations
    /// </summary>
    public class CompletedQuizRepo : ICompletedQuizRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompletedQuizRepo> _logger;

        public CompletedQuizRepo(
            AppDbContext context,
            ILogger<CompletedQuizRepo> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// Gets all completed quiz questions for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of completed quiz questions</returns>
        public async Task<List<CompletedQuiz>> GetByUserId(int userId)
        {
            try
            {
                _logger?.LogInformation("Getting completed quizzes for user {UserId}", userId);
                return await _context.CompletedQuizzes
                    .Where(cq => cq.UserId == userId)
                    .Include(cq => cq.QuizQuestion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting completed quizzes for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets IDs of all completed quiz questions for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of completed quiz question IDs</returns>
        public async Task<List<int>> GetCompletedQuizQuestionIdsByUserId(int userId)
        {
            try
            {
                _logger?.LogInformation("Getting completed quiz question IDs for user {UserId}", userId);
                return await _context.CompletedQuizzes
                    .Where(cq => cq.UserId == userId)
                    .Select(cq => cq.QuizQuestionId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting completed quiz question IDs for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Checks if a quiz question has been completed by a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="quizQuestionId">ID of the quiz question</param>
        /// <returns>True if completed, false otherwise</returns>
        public async Task<bool> IsQuizQuestionCompletedByUser(int userId, int quizQuestionId)
        {
            try
            {
                _logger?.LogInformation("Checking if quiz question {QuizQuestionId} is completed by user {UserId}", quizQuestionId, userId);
                return await _context.CompletedQuizzes
                    .AnyAsync(cq => cq.UserId == userId && cq.QuizQuestionId == quizQuestionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if quiz question {QuizQuestionId} is completed by user {UserId}", quizQuestionId, userId);
                throw;
            }
        }

        /// <summary>
        /// Marks a quiz question as completed by a user
        /// </summary>
        /// <param name="completedQuiz">Completed quiz information</param>
        /// <returns>The created completed quiz record</returns>
        public async Task<CompletedQuiz> MarkAsCompleted(CompletedQuiz completedQuiz)
        {
            try
            {
                _logger?.LogInformation("START: MarkAsCompleted - UserId={UserId}, QuizQuestionId={QuizQuestionId}, WasCorrect={WasCorrect}",
                    completedQuiz.UserId, completedQuiz.QuizQuestionId, completedQuiz.WasCorrect);

                // Check if already completed
                _logger?.LogInformation("Checking if record already exists");
                var existingRecord = await _context.CompletedQuizzes
                    .FirstOrDefaultAsync(cq => cq.UserId == completedQuiz.UserId && cq.QuizQuestionId == completedQuiz.QuizQuestionId);

                if (existingRecord != null)
                {
                    _logger?.LogInformation("Record already exists: Id={Id}, UserId={UserId}, QuizQuestionId={QuestionId}, WasCorrect={WasCorrect}",
                        existingRecord.Id, existingRecord.UserId, existingRecord.QuizQuestionId, existingRecord.WasCorrect);
                    return existingRecord;
                }

                // Add new record
                _logger?.LogInformation("Record does not exist, adding new record");

                try
                {
                    await _context.CompletedQuizzes.AddAsync(completedQuiz);
                    _logger?.LogInformation("Added to DbSet, now saving changes");
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("Changes saved successfully. New record Id={Id}", completedQuiz.Id);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error saving CompletedQuiz entity: {ErrorMessage}", ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logger?.LogError("Inner exception: {InnerError}", ex.InnerException.Message);
                    }
                    throw;
                }

                _logger?.LogInformation("Successfully marked quiz question {QuizQuestionId} as completed by user {UserId}",
                    completedQuiz.QuizQuestionId, completedQuiz.UserId);

                return completedQuiz;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in MarkAsCompleted: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}