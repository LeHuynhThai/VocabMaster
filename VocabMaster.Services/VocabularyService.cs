using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Services
{
    /// <summary>
    /// Service for vocabulary operations
    /// </summary>
    public class VocabularyService : IVocabularyService
    {
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly ILogger<VocabularyService> _logger;
        private readonly IMemoryCache _cache;
        private const string LearnedWordsCacheKey = "LearnedWords_";
        private const int CacheExpirationMinutes = 15;

        /// <summary>
        /// Initializes a new instance of the VocabularyService
        /// </summary>
        /// <param name="vocabularyRepository">Repository for vocabulary operations</param>
        /// <param name="learnedWordRepository">Repository for learned words operations</param>
        /// <param name="logger">Logger for the service</param>
        /// <param name="cache">Memory cache for improved performance</param>
        public VocabularyService(
            IVocabularyRepo vocabularyRepository,
            ILearnedWordRepo learnedWordRepository,
            ILogger<VocabularyService> logger,
            IMemoryCache cache = null)
        {
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

        /// <summary>
        /// Adds a word to the user's learned words list
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="word">Word to add to learned list</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AddLearnedWord(int userId, string word)
        {
            try
            {
                _logger.LogInformation("Adding word '{Word}' to learned list for user {UserId}", word, userId);
                
                // Check if the word is already learned
                if (await IsWordLearned(userId, word))
                {
                    _logger.LogInformation("Word '{Word}' is already in the learned list for user {UserId}", word, userId);
                    return true;
                }
                
                // Create new learned word
                var learnedWord = new LearnedWord
                {
                    UserId = userId,
                    Word = word,
                };
                
                var result = await _learnedWordRepository.Add(learnedWord);
                
                // Invalidate cache
                InvalidateUserCache(userId);
                
                _logger.LogInformation("Successfully added word '{Word}' to learned list for user {UserId}", word, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding word '{Word}' to learned list for user {UserId}", word, userId);
                return false;
            }
        }
        
        /// <summary>
        /// Marks a word as learned for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="word">Word to mark as learned</param>
        /// <returns>Result of the operation with success status and data</returns>
        public async Task<MarkWordResult> MarkWordAsLearned(int userId, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return new MarkWordResult { Success = false, ErrorMessage = "Word cannot be empty" };
            }

            try
            {
                // Check if the word is already learned
                if (await IsWordLearned(userId, word))
                {
                    _logger.LogWarning("User {UserId} tried to mark already learned word: {Word}", userId, word);
                    return new MarkWordResult { Success = false, ErrorMessage = "This word is already marked as learned" };
                }

                // Create new learned word
                var learnedWord = new LearnedWord
                {
                    UserId = userId,
                    Word = word,
                };
                
                var result = await _learnedWordRepository.Add(learnedWord);
                
                // Invalidate cache
                InvalidateUserCache(userId);
                
                if (result)
                {
                    _logger.LogInformation("Successfully marked word '{Word}' as learned for user {UserId}", word, userId);
                    return new MarkWordResult { Success = true, Data = learnedWord };
                }
                else
                {
                    _logger.LogWarning("Failed to mark word as learned for user {UserId}: {Word}", userId, word);
                    return new MarkWordResult { Success = false, ErrorMessage = "Failed to save learned word. Please try again." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word as learned for user {UserId}: {Word}", userId, word);
                return new MarkWordResult { Success = false, ErrorMessage = "An error occurred. Please try again." };
            }
        }
        
        /// <summary>
        /// Gets all learned words for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of learned words</returns>
        public async Task<List<LearnedWord>> GetUserLearnedVocabularies(int userId)
        {
            try
            {
                _logger.LogInformation("Getting learned words for user {UserId}", userId);
                return await _learnedWordRepository.GetByUserId(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned words for user {UserId}", userId);
                return new List<LearnedWord>();
            }
        }
        
        
        /// <summary>
        /// Removes a learned word for a specific user by ID
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="wordId">ID of the learned word to remove</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RemoveLearnedWordById(int userId, int wordId)
        {
            try
            {
                _logger.LogInformation("Removing learned word with ID {WordId} for user {UserId}", wordId, userId);
                
                // Verify the word belongs to the user
                var learnedWord = await _learnedWordRepository.GetById(wordId);
                if (learnedWord == null || learnedWord.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} tried to remove learned word with ID {WordId} that doesn't exist or doesn't belong to them", userId, wordId);
                    return false;
                }
                
                var result = await _learnedWordRepository.Delete(wordId);
                
                // Invalidate cache
                InvalidateUserCache(userId);
                
                _logger.LogInformation("Successfully removed learned word with ID {WordId} for user {UserId}", wordId, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing learned word by ID {WordId} for user {UserId}", wordId, userId);
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a word is in the user's learned words list
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="word">Word to check</param>
        /// <returns>True if the word is learned, false otherwise</returns>
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
        
        /// <summary>
        /// Invalidates the user's cache
        /// </summary>
        /// <param name="userId">ID of the user</param>
        private void InvalidateUserCache(int userId)
        {
            if (_cache != null)
            {
                string cacheKey = $"{LearnedWordsCacheKey}{userId}";
                _cache.Remove(cacheKey);
                
                // Also remove random word cache
                _cache.Remove($"RandomWord_{userId}");
            }
        }

        /// <summary>
        /// Gets a learned word by its ID for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="wordId">ID of the learned word</param>
        /// <returns>The learned word or null if not found</returns>
        public async Task<LearnedWord> GetLearnedWordById(int userId, int wordId)
        {
            try
            {
                _logger.LogInformation("Getting learned word with ID {WordId} for user {UserId}", wordId, userId);
                
                var learnedWord = await _learnedWordRepository.GetById(wordId);
                
                // Check if the word exists and belongs to the user
                if (learnedWord == null || learnedWord.UserId != userId)
                {
                    _logger.LogWarning("Learned word with ID {WordId} not found or doesn't belong to user {UserId}", wordId, userId);
                    return null;
                }
                
                _logger.LogInformation("Successfully retrieved learned word with ID {WordId} for user {UserId}", wordId, userId);
                return learnedWord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned word with ID {WordId} for user {UserId}", wordId, userId);
                return null;
            }
        }
    }
}


