using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
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

        public async Task<CompletedQuiz> MarkAsCompleted(CompletedQuiz completedQuiz)
        {
            try
            {
                _logger?.LogInformation("START: MarkAsCompleted - UserId={UserId}, QuizQuestionId={QuizQuestionId}, WasCorrect={WasCorrect}",
                    completedQuiz.UserId, completedQuiz.QuizQuestionId, completedQuiz.WasCorrect);

                _logger?.LogInformation("Checking if record already exists");
                var existingRecord = await _context.CompletedQuizzes
                    .FirstOrDefaultAsync(cq => cq.UserId == completedQuiz.UserId && cq.QuizQuestionId == completedQuiz.QuizQuestionId);

                if (existingRecord != null)
                {
                    _logger?.LogInformation("Record already exists: Id={Id}, UserId={UserId}, QuizQuestionId={QuestionId}, WasCorrect={WasCorrect}",
                        existingRecord.Id, existingRecord.UserId, existingRecord.QuizQuestionId, existingRecord.WasCorrect);
                    return existingRecord;
                }

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
        
        public async Task<(List<CompletedQuiz> Items, int TotalCount)> GetPaginatedCorrectQuizzes(int userId, int pageNumber, int pageSize)
        {
            try
            {
                _logger?.LogInformation("Getting paginated correct quizzes for user {UserId}, page {PageNumber}, size {PageSize}", 
                    userId, pageNumber, pageSize);
                
                var query = _context.CompletedQuizzes
                    .Where(cq => cq.UserId == userId && cq.WasCorrect)
                    .Include(cq => cq.QuizQuestion)
                    .OrderByDescending(cq => cq.CompletedAt);
                
                int totalCount = await query.CountAsync();
                _logger?.LogInformation("Total count of correct quizzes for user {UserId}: {Count}", userId, totalCount);
                
                if (totalCount == 0)
                {
                    _logger?.LogInformation("No correct quizzes found for user {UserId}", userId);
                    return (new List<CompletedQuiz>(), 0);
                }
                
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                _logger?.LogInformation("Retrieved {Count} correct quizzes for user {UserId} (total: {Total})",
                    items.Count, userId, totalCount);
                
                foreach (var item in items)
                {
                    if (item.QuizQuestion == null)
                    {
                        _logger?.LogWarning("QuizQuestion is null for CompletedQuiz {Id}, QuizQuestionId: {QuizId}", 
                            item.Id, item.QuizQuestionId);
                        
                        item.QuizQuestion = await _context.QuizQuestions.FindAsync(item.QuizQuestionId);
                        if (item.QuizQuestion == null)
                        {
                            _logger?.LogError("Failed to load QuizQuestion {Id} for CompletedQuiz {CompletedId}", 
                                item.QuizQuestionId, item.Id);
                        }
                    }
                }
                
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting paginated correct quizzes for user {UserId}: {Message}", 
                    userId, ex.Message);
                throw;
            }
        }
    }
}