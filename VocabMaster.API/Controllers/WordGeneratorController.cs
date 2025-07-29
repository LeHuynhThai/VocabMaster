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
                    _logger.LogInformation("Cached word has Vietnamese: {HasVietnamese}", !string.IsNullOrEmpty(cachedWord.Vietnamese) ? "Yes" : "No");
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

                // Debug Vietnamese translation
                _logger.LogInformation("Random word {Word} has Vietnamese translation: {HasVietnamese}", 
                    randomWord.Word, !string.IsNullOrEmpty(randomWord.Vietnamese) ? "Yes" : "No");
                
                // Convert to simplified response
                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned, randomWord.Vietnamese);
                
                // Debug response
                _logger.LogInformation("Response for word {Word} has Vietnamese: {HasVietnamese}", 
                    response.Word, !string.IsNullOrEmpty(response.Vietnamese) ? "Yes" : "No");
                
                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _cache.Set(cacheKey, response, cacheOptions);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return StatusCode(500, new { message = "An error occurred while getting a random word" });
            }
        }

        /// <summary>
        /// Gets a new random word, bypassing cache
        /// </summary>
        /// <returns>A simplified random word with its definition</returns>
        /// <response code="200">Returns the random word with its definition</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no words are available or all words have been learned</response>
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

                // Invalidate cache
                string cacheKey = $"{RandomWordCacheKey}{userId}";
                if (_cache != null)
                {
                    _cache.Remove(cacheKey);
                }

                _logger.LogInformation("Getting new random word for user {UserId}", userId);
                var randomWord = await _dictionaryService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    _logger.LogInformation("No random word found for user {UserId}", userId);
                    return NotFound(new { message = "No word found or all words have been learned" });
                }

                // Check if the word is already learned
                bool isLearned = await _vocabularyService.IsWordLearned(userId, randomWord.Word);

                // Debug Vietnamese translation
                _logger.LogInformation("New random word {Word} has Vietnamese translation: {HasVietnamese}", 
                    randomWord.Word, !string.IsNullOrEmpty(randomWord.Vietnamese) ? "Yes" : "No");
                
                // Convert to simplified response
                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned, randomWord.Vietnamese);
                
                // Debug response
                _logger.LogInformation("Response for new word {Word} has Vietnamese: {HasVietnamese}", 
                    response.Word, !string.IsNullOrEmpty(response.Vietnamese) ? "Yes" : "No");
                
                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _cache.Set(cacheKey, response, cacheOptions);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new random word");
                return StatusCode(500, new { message = "An error occurred while getting a new random word" });
            }
        }

        /// <summary>
        /// Looks up a word in the dictionary
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>A simplified word with its definition</returns>
        /// <response code="200">Returns the word with its definition</response>
        /// <response code="400">If the word is null or empty</response>
        /// <response code="404">If the word is not found</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpGet("lookup/{word}")]
        [Authorize]
        public async Task<IActionResult> Lookup(string word)
        {
            try
            {
                _logger.LogInformation("Looking up definition for word: {Word}", word);
                
                // Get the definition from cache or API
                var definition = await _dictionaryService.GetWordDefinitionFromCache(word);
                
                if (definition == null)
                {
                    _logger.LogWarning("No definition found for word: {Word}", word);
                    return NotFound(new { message = $"No definition found for word: {word}" });
                }
                
                // Debug Vietnamese translation
                _logger.LogInformation("Lookup word {Word} has Vietnamese translation: {HasVietnamese}", 
                    definition.Word, !string.IsNullOrEmpty(definition.Vietnamese) ? "Yes" : "No");
                
                _logger.LogInformation("Successfully retrieved definition for word: {Word}", word);
                return Ok(definition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up definition for word: {Word}", word);
                return StatusCode(500, new { message = "An error occurred while looking up the definition" });
            }
        }

        /// <summary>
        /// Checks if a word is learned by the current user
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <returns>Whether the word is learned</returns>
        /// <response code="200">Returns whether the word is learned</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("islearned/{word}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> IsLearned(string word)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID from claims: {UserId}", userId);
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            try
            {
                _logger.LogInformation("Checking if word {Word} is learned by user {UserId}", word, userId);
                bool isLearned = await _vocabularyService.IsWordLearned(userId, word);
                
                return Ok(new { isLearned });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if word {Word} is learned", word);
                return StatusCode(500, new { message = "An error occurred while checking if the word is learned" });
            }
        }

        /// <summary>
        /// Adds a word to the user's learned words list
        /// </summary>
        /// <param name="word">The word to add</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">If the word was added successfully</response>
        /// <response code="400">If the word is null or empty</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpPost("learned/{word}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddLearnedWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest(new { message = "Word cannot be empty" });
            }

            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID from claims: {UserId}", userId);
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            try
            {
                _logger.LogInformation("Adding word {Word} to learned list for user {UserId}", word, userId);
                var result = await _vocabularyService.MarkWordAsLearned(userId, word);
                
                if (result.Success)
                {
                    // Invalidate random word cache
                    if (_cache != null)
                    {
                        string cacheKey = $"{RandomWordCacheKey}{userId}";
                        _cache.Remove(cacheKey);
                    }
                    
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest(new { message = result.ErrorMessage ?? "Cannot mark word as learned" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding word {Word} to learned list", word);
                return StatusCode(500, new { message = "An error occurred while adding the word to learned list" });
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
