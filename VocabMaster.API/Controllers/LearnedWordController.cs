using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.DTOs;

namespace VocabMaster.API.Controllers
{
    /// <summary>
    /// Controller for managing learned words
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LearnedWordController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;
        private readonly IDictionaryService _dictionaryService;
        private readonly ILogger<LearnedWordController> _logger;
        private readonly IMemoryCache _cache;
        private const string LearnedWordsListCacheKey = "LearnedWordsList_";
        private const int CacheExpirationMinutes = 5;

        /// <summary>
        /// Initializes a new instance of the LearnedWordController
        /// </summary>
        /// <param name="vocabularyService">Service for vocabulary operations</param>
        /// <param name="dictionaryService">Service for dictionary operations</param>
        /// <param name="logger">Logger for the controller</param>
        /// <param name="cache">Memory cache for improved performance</param>
        public LearnedWordController(
            IVocabularyService vocabularyService,
            IDictionaryService dictionaryService,
            ILogger<LearnedWordController> logger,
            IMemoryCache cache = null)
        {
            _vocabularyService = vocabularyService ?? throw new ArgumentNullException(nameof(vocabularyService));
            _dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

        /// <summary>
        /// Gets the user's learned words
        /// </summary>
        /// <returns>List of learned words</returns>
        /// <response code="200">Returns the list of learned words</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<LearnedWordResponseDto>), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetLearnedWords()
        {
            try
            {
                // Get UserId from Claims
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID from claims");
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                // Try to get from cache first
                string cacheKey = $"{LearnedWordsListCacheKey}{userId}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out List<LearnedWordResponseDto> cachedWords))
                {
                    _logger.LogInformation("Retrieved learned words from cache for user {UserId}", userId);
                    return Ok(cachedWords);
                }

                // Get learned words
                _logger.LogInformation("Getting learned words for user {UserId}", userId);
                var learnedWords = await _vocabularyService.GetUserLearnedVocabularies(userId);

                // if no words, return empty list instead of error
                if (learnedWords == null)
                {
                    _logger.LogInformation("No learned words found for user {UserId}", userId);
                    return Ok(new List<LearnedWordResponseDto>());
                }
                
                // Convert to response DTOs
                var response = learnedWords.Select(lw => new LearnedWordResponseDto
                {
                    Id = lw.Id,
                    Word = lw.Word,
                }).ToList();
                
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
                _logger.LogError(ex, "Error loading learned words");
                // return empty list instead of error 500 to avoid client error
                return Ok(new List<LearnedWordResponseDto>());
            }
        }

        /// <summary>
        /// Gets details of a specific learned word
        /// </summary>
        /// <param name="id">ID of the learned word</param>
        /// <returns>Detailed information about the learned word</returns>
        /// <response code="200">Returns the learned word details</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the word is not found</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VocabularyResponseDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetLearnedWord(int id)
        {
            try
            {
                // Get UserId from Claims
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID from claims");
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                // Get the learned word
                var learnedWord = await _vocabularyService.GetLearnedWordById(userId, id);
                if (learnedWord == null)
                {
                    return NotFound(new { message = "Learned word not found" });
                }

                // Get detailed information about the word
                var wordDetails = await _dictionaryService.GetWordDefinition(learnedWord.Word);
                if (wordDetails == null)
                {
                    // Return basic information if detailed information is not available
                    return Ok(new LearnedWordResponseDto
                    {
                        Id = learnedWord.Id,
                        Word = learnedWord.Word,
                    });
                }

                // Convert to vocabulary response
                var response = VocabularyResponseDto.FromDictionaryResponse(wordDetails, learnedWord.Id, true);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned word details");
                return StatusCode(500, new { message = "An error occurred while retrieving learned word details" });
            }
        }

        /// <summary>
        /// Adds a word to the user's learned words list
        /// </summary>
        /// <param name="request">Request containing the word to add</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">If the word was added successfully</response>
        /// <response code="400">If the request is invalid or the word is already learned</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddLearnedWord([FromBody] AddLearnedWordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Word))
            {
                return BadRequest(new { message = "Word cannot be empty" });
            }

            // Get UserId from Claims
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID from claims");
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            try
            {
                _logger.LogInformation("Adding word '{Word}' to learned list for user {UserId}", request.Word, userId);
                var result = await _vocabularyService.MarkWordAsLearned(userId, request.Word.Trim());
                
                if (result.Success)
                {
                    // Invalidate cache
                    InvalidateCache(userId);
                    
                    return Ok(new { 
                        id = result.Data?.Id ?? 0, 
                        word = request.Word.Trim(),
                    });
                }
                else
                {
                    return BadRequest(new { message = result.ErrorMessage ?? "Cannot mark word as learned. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word as learned: {Word}", request.Word);
                return StatusCode(500, new { message = "An error occurred while adding the word to learned list" });
            }
        }

        /// <summary>
        /// Removes a learned word by its ID
        /// </summary>
        /// <param name="id">ID of the learned word to remove</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">If the word was removed successfully</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the word is not found or doesn't belong to the user</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteLearnedWord(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid word ID" });
            }

            // Get UserId from Claims
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID from claims");
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            try
            {
                _logger.LogInformation("Removing learned word with ID {WordId} for user {UserId}", id, userId);
                var result = await _vocabularyService.RemoveLearnedWordById(userId, id);
                
                if (result)
                {
                    // Invalidate cache
                    InvalidateCache(userId);
                    
                    return Ok(new { success = true });
                }
                else
                {
                    return NotFound(new { message = "Word not found or you don't have permission to remove it." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing learned word: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while removing the word from learned list" });
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
        
        /// <summary>
        /// Invalidates the user's cache
        /// </summary>
        /// <param name="userId">ID of the user</param>
        private void InvalidateCache(int userId)
        {
            if (_cache != null)
            {
                string cacheKey = $"{LearnedWordsListCacheKey}{userId}";
                _cache.Remove(cacheKey);
            }
        }
    }

    /// <summary>
    /// Request model for adding a learned word
    /// </summary>
    public class AddLearnedWordRequest
    {
        /// <summary>
        /// Word to add to learned list
        /// </summary>
        public string Word { get; set; }
    }
    
    /// <summary>
    /// Response model for learned words
    /// </summary>
    public class LearnedWordResponseDto
    {
        /// <summary>
        /// ID of the learned word
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The word text
        /// </summary>
        public string Word { get; set; }
        
        /// <summary>
        /// Date and time when the word was learned
        /// </summary>
        public DateTime LearnedDate { get; set; }
    }
}
