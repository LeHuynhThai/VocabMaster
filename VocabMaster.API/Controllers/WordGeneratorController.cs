using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Dictionary;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WordGeneratorController : ControllerBase
    {
        private readonly IRandomWordService _randomWordService;
        private readonly IDictionaryLookupService _dictionaryLookupService;
        private readonly ILearnedWordService _learnedWordService;
        private readonly IWordStatusService _wordStatusService;
        private readonly ILogger<WordGeneratorController> _logger;
        private readonly IMemoryCache _cache;
        private const string RandomWordCacheKey = "RandomWord_";
        private const string LookupCacheKey = "Lookup_";
        private const int CacheExpirationMinutes = 30;

        public WordGeneratorController(
            IRandomWordService randomWordService,
            IDictionaryLookupService dictionaryLookupService,
            ILearnedWordService learnedWordService,
            IWordStatusService wordStatusService,
            ILogger<WordGeneratorController> logger,
            IMemoryCache cache = null)
        {
            _randomWordService = randomWordService ?? throw new ArgumentNullException(nameof(randomWordService));
            _dictionaryLookupService = dictionaryLookupService ?? throw new ArgumentNullException(nameof(dictionaryLookupService));
            _learnedWordService = learnedWordService ?? throw new ArgumentNullException(nameof(learnedWordService));
            _wordStatusService = wordStatusService ?? throw new ArgumentNullException(nameof(wordStatusService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

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
                    return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
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
                var randomWord = await _randomWordService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    _logger.LogInformation("No random word found for user {UserId}", userId);
                    return NotFound(new { message = "No word found or all words have been learned" });
                }

                bool isLearned = await _wordStatusService.IsWordLearned(userId, randomWord.Word);

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
                return StatusCode(500, new
                {
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi lấy từ ngẫu nhiên",
                    details = ex.Message
                });
            }
        }

        [HttpGet("newrandomword")]
        [ProducesResponseType(typeof(VocabularyResponseDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRandomWordExcludeLearned()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID from claims when getting random word excluding learned");
                    return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
                }

                // Invalidate cache
                string cacheKey = $"{RandomWordCacheKey}{userId}";
                if (_cache != null)
                {
                    _cache.Remove(cacheKey);
                    _logger.LogInformation("Cache invalidated for user {UserId}", userId);
                }

                _logger.LogInformation("Getting random word excluding learned for user {UserId}", userId);
                var randomWord = await _randomWordService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    _logger.LogInformation("No random word found for user {UserId} - all words may be learned", userId);
                    return NotFound(new { message = "No word found or all words have been learned" });
                }

                bool isLearned = await _wordStatusService.IsWordLearned(userId, randomWord.Word);

                // Convert to simplified response
                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned, randomWord.Vietnamese);

                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);

                    _cache.Set(cacheKey, response, cacheOptions);
                    _logger.LogInformation("Random word cached for user {UserId}", userId);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word excluding learned for user ID {UserId}", GetUserIdFromClaims());
                return StatusCode(500, new
                {
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi lấy từ ngẫu nhiên chưa học",
                    details = ex.Message
                });
            }
        }

        [HttpGet("lookup/{word}")]
        [Authorize]
        public async Task<IActionResult> Lookup(string word)
        {
            try
            {
                _logger.LogInformation("Looking up definition for word: {Word}", word);

                // Get the definition from cache or API
                var definition = await _dictionaryLookupService.GetWordDefinitionFromCache(word);

                if (definition == null)
                {
                    _logger.LogWarning("No definition found for word: {Word}", word);
                    return NotFound(new
                    {
                        error = "word_not_found",
                        message = $"Không tìm thấy định nghĩa cho từ: {word}"
                    });
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
                return StatusCode(500, new
                {
                    error = "lookup_error",
                    message = $"Đã xảy ra lỗi khi tra cứu định nghĩa cho từ: {word}",
                    details = ex.Message
                });
            }
        }

        [HttpGet("islearned/{word}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> IsLearned(string word)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID from claims: {UserId}", userId);
                return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
            }

            try
            {
                _logger.LogInformation("Checking if word {Word} is learned by user {UserId}", word, userId);
                bool isLearned = await _wordStatusService.IsWordLearned(userId, word);

                return Ok(new { isLearned });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if word {Word} is learned", word);
                return StatusCode(500, new
                {
                    error = "check_error",
                    message = $"Đã xảy ra lỗi khi kiểm tra từ {word} đã học hay chưa",
                    details = ex.Message
                });
            }
        }

        [HttpPost("learned/{word}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddLearnedWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest(new
                {
                    error = "invalid_input",
                    message = "Từ không được để trống"
                });
            }

            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid user ID from claims: {UserId}", userId);
                return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
            }

            try
            {
                _logger.LogInformation("Adding word {Word} to learned list for user {UserId}", word, userId);
                var result = await _learnedWordService.MarkWordAsLearned(userId, word);

                if (result.Success)
                {
                    if (_cache != null)
                    {
                        string cacheKey = $"{RandomWordCacheKey}{userId}";
                        _cache.Remove(cacheKey);
                    }

                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest(new
                    {
                        error = "mark_error",
                        message = result.ErrorMessage ?? "Không thể đánh dấu từ đã học"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding word {Word} to learned list", word);
                return StatusCode(500, new
                {
                    error = "add_error",
                    message = $"Đã xảy ra lỗi khi thêm từ {word} vào danh sách từ đã học",
                    details = ex.Message
                });
            }
        }

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
