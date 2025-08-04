using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    // Repository for learned word operations
    public class LearnedWordRepo : ILearnedWordRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LearnedWordRepo> _logger;

        // Initializes a new instance of the LearnedWordRepo
        public LearnedWordRepo(
            AppDbContext context,
            ILogger<LearnedWordRepo> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        // Gets a learned word by its ID
        public async Task<LearnedWord> GetById(int id)
        {
            try
            {
                _logger?.LogInformation("Getting learned word with ID: {Id}", id);
                return await _context.LearnedVocabularies.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting learned word with ID: {Id}", id);
                throw;
            }
        }

        // Gets all learned words for a specific user
        public async Task<List<LearnedWord>> GetByUserId(int userId)
        {
            try
            {
                _logger?.LogInformation("Getting learned words for user: {UserId}", userId);

                // Use a more efficient query with Include for eager loading
                return await _context.LearnedVocabularies
                    .Where(lv => lv.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting learned words for user: {UserId}", userId);
                return new List<LearnedWord>();
            }
        }

        // Adds a new learned word
        public async Task<bool> Add(LearnedWord learnedWord)
        {
            try
            {
                if (learnedWord == null)
                {
                    _logger?.LogWarning("Attempted to add null learned word");
                    return false;
                }

                _logger?.LogInformation("Adding learned word: {Word} for user: {UserId}",
                    learnedWord.Word, learnedWord.UserId);

                await _context.LearnedVocabularies.AddAsync(learnedWord);
                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully added learned word with ID: {Id}", learnedWord.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding learned word: {Word} for user: {UserId}",
                    learnedWord?.Word, learnedWord?.UserId);
                return false;
            }
        }

        // Deletes a learned word by its ID
        public async Task<bool> Delete(int id)
        {
            try
            {
                _logger?.LogInformation("Deleting learned word with ID: {Id}", id);

                var learnedWord = await _context.LearnedVocabularies.FindAsync(id);
                if (learnedWord == null)
                {
                    _logger?.LogWarning("Learned word with ID: {Id} not found for deletion", id);
                    return false;
                }

                _context.LearnedVocabularies.Remove(learnedWord);
                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully deleted learned word with ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting learned word with ID: {Id}", id);
                return false;
            }
        }

        // Removes a learned word for a specific user by word text
        public async Task<bool> RemoveByWord(int userId, string word)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    _logger?.LogWarning("Attempted to remove learned word with empty word text for user: {UserId}", userId);
                    return false;
                }

                _logger?.LogInformation("Removing learned word: {Word} for user: {UserId}", word, userId);

                // Find the learned word to remove
                var learnedWord = await _context.LearnedVocabularies
                    .FirstOrDefaultAsync(lv =>
                        lv.UserId == userId &&
                        EF.Functions.Like(lv.Word, word));

                if (learnedWord == null)
                {
                    _logger?.LogWarning("Learned word: {Word} not found for user: {UserId}", word, userId);
                    return false;
                }

                // Remove the learned word
                _context.LearnedVocabularies.Remove(learnedWord);
                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully removed learned word: {Word} for user: {UserId}", word, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing learned word: {Word} for user: {UserId}", word, userId);
                return false;
            }
        }

        // Gets paginated learned words for a specific user
        public async Task<(List<LearnedWord> Items, int TotalCount)> GetPaginatedByUserId(int userId, int pageNumber, int pageSize)
        {
            try
            {
                _logger?.LogInformation("Getting paginated learned words for user {UserId}, page {Page}, size {Size}", 
                    userId, pageNumber, pageSize);
                
                var query = _context.LearnedVocabularies
                    .Where(lw => lw.UserId == userId)
                    .OrderByDescending(lw => lw.LearnedAt);
                    
                int totalCount = await query.CountAsync();
                
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                    
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting paginated learned words for user {UserId}", userId);
                throw;
            }
        }
    }
}