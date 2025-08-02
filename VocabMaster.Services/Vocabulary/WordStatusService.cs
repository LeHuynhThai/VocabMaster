using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.Services.Vocabulary
{
    public class WordStatusService : IWordStatusService
    {
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly ILogger<WordStatusService> _logger;
        private readonly IMemoryCache _cache;
        private const string LearnedWordsCacheKey = "LearnedWords_";
        private const int CacheExpirationMinutes = 15;

        public WordStatusService(
            ILearnedWordRepo learnedWordRepository,
            ILogger<WordStatusService> logger,
            IMemoryCache cache = null)
        {
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

        public async Task<bool> IsWordLearned(int userId, string word)
        {
            try
            {
                // Try to get from cache first
                string cacheKey = $"{LearnedWordsCacheKey}{userId}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out HashSet<string> learnedWords))
                {
                    return learnedWords.Contains(word, StringComparer.OrdinalIgnoreCase);
                }

                // Get all learned words for the user
                var userLearnedWords = await _learnedWordRepository.GetByUserId(userId);

                // Check if the word is in the learned list
                bool isLearned = userLearnedWords.Any(lw =>
                    string.Equals(lw.Word, word, StringComparison.OrdinalIgnoreCase));

                // Cache the learned words for future checks
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

        public void InvalidateUserCache(int userId)
        {
            if (_cache != null)
            {
                string cacheKey = $"{LearnedWordsCacheKey}{userId}";
                _cache.Remove(cacheKey);

                // Also remove random word cache
                _cache.Remove($"RandomWord_{userId}");
            }
        }
    }
} 