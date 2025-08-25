using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    // Repository thao tác với dữ liệu từ đã học của người dùng
    public class LearnedWordRepo : ILearnedWordRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LearnedWordRepo> _logger;

        // Hàm khởi tạo repository, inject context và logger
        public LearnedWordRepo(
            AppDbContext context,
            ILogger<LearnedWordRepo> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        // Lấy từ đã học theo Id
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

        // Lấy toàn bộ từ đã học của một user
        public async Task<List<LearnedWord>> GetByUserId(int userId)
        {
            try
            {
                _logger?.LogInformation("Getting learned words for user: {UserId}", userId);

                // Truy vấn hiệu quả, có thể dùng Include nếu cần load thêm navigation property
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

        // Thêm một từ đã học mới
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

        // Xóa một từ đã học theo Id
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

        // Xóa một từ đã học của user theo từ (word)
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

                // Tìm bản ghi cần xóa
                var learnedWord = await _context.LearnedVocabularies
                    .FirstOrDefaultAsync(lv =>
                        lv.UserId == userId &&
                        EF.Functions.Like(lv.Word, word));

                if (learnedWord == null)
                {
                    _logger?.LogWarning("Learned word: {Word} not found for user: {UserId}", word, userId);
                    return false;
                }

                // Xóa bản ghi
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

        // Lấy danh sách từ đã học có phân trang cho user
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