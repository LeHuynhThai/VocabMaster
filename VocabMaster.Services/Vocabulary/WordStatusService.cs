using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.Services.Vocabulary
{
    // Service kiểm tra trạng thái đã học của từ, cache kết quả để tăng hiệu năng
    public class WordStatusService : IWordStatusService
    {
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly ILogger<WordStatusService> _logger;
        private readonly IMemoryCache _cache;
        private const string LearnedWordsCacheKey = "LearnedWords_";
        private const int CacheExpirationMinutes = 15;

        // Hàm khởi tạo service, inject repository, logger, cache
        public WordStatusService(
            ILearnedWordRepo learnedWordRepository,
            ILogger<WordStatusService> logger,
            IMemoryCache cache = null)
        {
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

        // Kiểm tra một từ đã học hay chưa (ưu tiên lấy từ cache)
        public async Task<bool> IsWordLearned(int userId, string word)
        {
            try
            {
                // Thử lấy từ cache trước
                string cacheKey = $"{LearnedWordsCacheKey}{userId}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out HashSet<string> learnedWords))
                {
                    return learnedWords.Contains(word, StringComparer.OrdinalIgnoreCase);
                }

                // Lấy toàn bộ từ đã học của user
                var userLearnedWords = await _learnedWordRepository.GetByUserId(userId);

                // Kiểm tra từ có trong danh sách đã học không
                bool isLearned = userLearnedWords.Any(lw =>
                    string.Equals(lw.Word, word, StringComparison.OrdinalIgnoreCase));

                // Cache lại danh sách từ đã học cho lần kiểm tra sau
                if (_cache != null)
                {
                    var wordSet = new HashSet<string>(
                        userLearnedWords.Select(lw => lw.Word),
                        StringComparer.OrdinalIgnoreCase);

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);

                    _cache.Set(cacheKey, wordSet, cacheOptions);
                }

                return isLearned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if word '{Word}' is learned for user {UserId}", word, userId);
                return false;
            }
        }

        // Xóa cache trạng thái đã học của user (khi thêm/xóa từ)
        public void InvalidateUserCache(int userId)
        {
            if (_cache != null)
            {
                string cacheKey = $"{LearnedWordsCacheKey}{userId}";
                _cache.Remove(cacheKey);

                // Xóa luôn cache random word nếu có
                _cache.Remove($"RandomWord_{userId}");
            }
        }
    }
}