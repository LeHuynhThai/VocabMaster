using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.DTOs;

namespace VocabMaster.API.Controllers
{
    /// <summary>
    /// Controller for word generation and dictionary lookup operations
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WordGeneratorController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly IVocabularyService _vocabularyService;
        private readonly ILogger<WordGeneratorController> _logger;
        private readonly IMemoryCache _cache;
        private const string RandomWordCacheKey = "RandomWord_";
        private const string LookupCacheKey = "Lookup_";
        private const int CacheExpirationMinutes = 30;

        /// <summary>
        /// Initializes a new instance of the WordGeneratorController
        /// </summary>
        /// <param name="dictionaryService">Service for dictionary operations</param>
        /// <param name="vocabularyService">Service for vocabulary operations</param>
        /// <param name="logger">Logger for the controller</param>
        /// <param name="cache">Memory cache for improved performance</param>
        public WordGeneratorController(
            IDictionaryService dictionaryService,
            IVocabularyService vocabularyService,
            ILogger<WordGeneratorController> logger,
            IMemoryCache cache = null)
        {
            _dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
            _vocabularyService = vocabularyService ?? throw new ArgumentNullException(nameof(vocabularyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

        /// <summary>
        /// Gets a random word excluding words already learned by the current user
        /// </summary>
        /// <returns>A simplified random word with its definition</returns>
        /// <response code="200">Returns the random word with its definition</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no words are available or all words have been learned</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpGet("getrandomword")]
        [ProducesResponseType(typeof(VocabularyResponseDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRandomWord()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID from claims: {UserId}", userId);
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                // Try to get from cache first
                string cacheKey = $"{RandomWordCacheKey}{userId}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out VocabularyResponseDto cachedWord))
                {
                    _logger.LogInformation("Retrieved random word from cache for user {UserId}", userId);
                    return Ok(cachedWord);
                }

                _logger.LogInformation("Getting random word for user {UserId}", userId);
                var randomWord = await _dictionaryService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    _logger.LogInformation("No random word found for user {UserId}", userId);
                    return NotFound(new { message = "No word found or all words have been learned" });
                }

                // Check if the word is already learned
                bool isLearned = await _vocabularyService.IsWordLearned(userId, randomWord.Word);

                // Convert to simplified response
                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned);
                
                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _cache.Set(cacheKey, response, cacheOptions);
                }

                _logger.LogInformation("Successfully retrieved random word '{Word}' for user {UserId}", 
                    randomWord.Word, userId);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating random word");
                return StatusCode(500, new { message = "An error occurred while retrieving a random word" });
            }
        }

        /// <summary>
        /// Gets a new random word, ignoring the cache
        /// </summary>
        /// <returns>A simplified random word with its definition</returns>
        /// <response code="200">Returns the random word with its definition</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no words are available</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpGet("getnewrandomword")]
        [ProducesResponseType(typeof(VocabularyResponseDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetNewRandomWord()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID from claims: {UserId}", userId);
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                // Clear the cache for this user
                string cacheKey = $"{RandomWordCacheKey}{userId}";
                if (_cache != null)
                {
                    _cache.Remove(cacheKey);
                    _logger.LogInformation("Cleared random word cache for user {UserId}", userId);
                }

                _logger.LogInformation("Getting new random word for user {UserId}", userId);
                var randomWord = await _dictionaryService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    _logger.LogInformation("No random word found for user {UserId}", userId);
                    return NotFound(new { message = "No word found" });
                }

                // Check if the word is already learned
                bool isLearned = await _vocabularyService.IsWordLearned(userId, randomWord.Word);

                // Convert to simplified response
                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned);
                
                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _cache.Set(cacheKey, response, cacheOptions);
                }

                _logger.LogInformation("Successfully retrieved new random word '{Word}' for user {UserId}", 
                    randomWord.Word, userId);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating new random word");
                return StatusCode(500, new { message = "An error occurred while retrieving a new random word" });
            }
        }

        /// <summary>
        /// Looks up the definition of a specific word
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>The simplified word definition</returns>
        /// <response code="200">Returns the word definition</response>
        /// <response code="400">If the word parameter is null or empty</response>
        /// <response code="404">If no definition is found for the word</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpGet("lookup/{word}")]
        [ProducesResponseType(typeof(VocabularyResponseDto), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Lookup(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return BadRequest(new { message = "Word cannot be empty" });
            }

            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID from claims");
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                // Normalize the word
                word = word.Trim().ToLowerInvariant();

                // Try to get from cache first
                string cacheKey = $"{LookupCacheKey}{word}_{userId}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out VocabularyResponseDto cachedDefinition))
                {
                    _logger.LogInformation("Retrieved definition for word '{Word}' from cache", word);
                    return Ok(cachedDefinition);
                }

                _logger.LogInformation("Looking up definition for word: {Word}", word);
                var definition = await _dictionaryService.GetWordDefinition(word);

                if (definition == null)
                {
                    _logger.LogInformation("No definition found for word: {Word}", word);
                    return NotFound(new { message = $"No definition found for word: {word}" });
                }

                // Check if the word is already learned
                bool isLearned = await _vocabularyService.IsWordLearned(userId, word);

                // Convert to simplified response
                var response = VocabularyResponseDto.FromDictionaryResponse(definition, 0, isLearned);
                
                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(24)) // Definitions don't change often
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _cache.Set(cacheKey, response, cacheOptions);
                }

                _logger.LogInformation("Successfully retrieved definition for word: {Word}", word);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting word definition: {Word}", word);
                return StatusCode(500, new { message = "An error occurred while looking up the word definition" });
            }
        }

        /// <summary>
        /// Checks if a word is already learned by the current user
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <returns>Whether the word is learned</returns>
        [HttpGet("islearned/{word}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> IsLearned(string word)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            try
            {
                var isLearned = await _vocabularyService.IsWordLearned(userId, word);
                return Ok(new { isLearned });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if word is learned: {Word}", word);
                return StatusCode(500, new { message = "An error occurred while checking if the word is learned" });
            }
        }
        
        /// <summary>
        /// Gets the user ID from the claims principal
        /// </summary>
        /// <returns>User ID or 0 if not found or invalid</returns>
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) ?? 
                              User.Claims.FirstOrDefault(c => c.Type == "UserId");
                              
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            
            _logger.LogWarning("UserId not found in claims or could not be parsed");
            return 0;
        }
    }
}
