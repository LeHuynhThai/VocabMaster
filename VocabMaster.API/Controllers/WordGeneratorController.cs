using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Dictionary;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.API.Controllers
{
    // Controller quản lý các API liên quan đến sinh từ vựng ngẫu nhiên, tra cứu từ, đánh dấu đã học...
    // Sử dụng các service để lấy từ ngẫu nhiên, kiểm tra trạng thái đã học, tra cứu từ điển, caching...
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WordGeneratorController : ControllerBase
    {
        // Service sinh từ ngẫu nhiên (loại trừ từ đã học)
        private readonly IRandomWordService _randomWordService;
        // Service tra cứu thông tin từ điển
        private readonly IDictionaryLookupService _dictionaryLookupService;
        // Service thao tác với từ đã học
        private readonly ILearnedWordService _learnedWordService;
        // Service kiểm tra trạng thái đã học của từ
        private readonly IWordStatusService _wordStatusService;
        // Ghi log cho controller
        private readonly ILogger<WordGeneratorController> _logger;
        // Bộ nhớ đệm (cache) trong bộ nhớ
        private readonly IMemoryCache _cache;
        // Key dùng cho cache từ ngẫu nhiên
        private const string RandomWordCacheKey = "RandomWord_";
        // Key dùng cho cache tra cứu từ
        private const string LookupCacheKey = "Lookup_";
        // Thời gian hết hạn cache (phút)
        private const int CacheExpirationMinutes = 30;

        // Hàm khởi tạo controller, inject các service cần thiết
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

        /// <summary>
        /// Lấy một từ vựng ngẫu nhiên mà user chưa học (ưu tiên lấy từ cache nếu có)
        /// </summary>
        [HttpGet("getrandomword")]
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

                // Thử lấy từ cache trước
                string cacheKey = $"{RandomWordCacheKey}{userId}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out VocabularyResponseDto cachedWord))
                {
                    _logger.LogInformation("Retrieved random word from cache for user {UserId}", userId);
                    _logger.LogInformation("Cached word has Vietnamese: {HasVietnamese}", !string.IsNullOrEmpty(cachedWord.Vietnamese) ? "Yes" : "No");
                    return Ok(cachedWord);
                }

                _logger.LogInformation("Getting random word for user {UserId}", userId);
                // Lấy từ ngẫu nhiên loại trừ từ đã học
                var randomWord = await _randomWordService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    _logger.LogInformation("No random word found for user {UserId}", userId);
                    return Ok(new { 
                        allLearned = true, 
                        message = "Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống."
                    });
                }

                // Kiểm tra từ này đã học chưa
                bool isLearned = await _wordStatusService.IsWordLearned(userId, randomWord.Word);

                _logger.LogInformation("Random word {Word} has Vietnamese translation: {HasVietnamese}",
                    randomWord.Word, !string.IsNullOrEmpty(randomWord.Vietnamese) ? "Yes" : "No");

                // Chuyển sang response đơn giản
                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned, randomWord.Vietnamese);

                // Debug response
                _logger.LogInformation("Response for word {Word} has Vietnamese: {HasVietnamese}",
                    response.Word, !string.IsNullOrEmpty(response.Vietnamese) ? "Yes" : "No");

                // Lưu vào cache
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

        /// <summary>
        /// Lấy một từ vựng ngẫu nhiên chưa học (luôn làm mới cache)
        /// </summary>
        [HttpGet("newrandomword")]
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

                // Xóa cache cũ
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
                    return Ok(new { 
                        allLearned = true, 
                        message = "Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống."
                    });
                }

                bool isLearned = await _wordStatusService.IsWordLearned(userId, randomWord.Word);

                // Chuyển sang response đơn giản
                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned, randomWord.Vietnamese);

                // Lưu vào cache
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

        /// <summary>
        /// Tra cứu thông tin từ vựng trong cơ sở dữ liệu
        /// </summary>
        [HttpGet("lookup/{word}")]
        [Authorize]
        public async Task<IActionResult> Lookup(string word)
        {
            try
            {
                _logger.LogInformation("Looking up definition for word: {Word}", word);

                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID from claims: {UserId}", userId);
                    return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
                }

                // Lấy định nghĩa từ database
                var definition = await _dictionaryLookupService.GetWordDefinitionFromDatabase(word);

                if (definition == null)
                {
                    _logger.LogWarning("No definition found for word: {Word}", word);
                    return NotFound(new
                    {
                        error = "word_not_found",
                        message = $"Không tìm thấy từ vựng trong cơ sở dữ liệu, vui lòng tìm kiếm từ khác"
                    });
                }

                // Kiểm tra từ đã học chưa
                bool isLearned = await _wordStatusService.IsWordLearned(userId, word);
                
                // Chuyển sang response đơn giản
                var response = VocabularyResponseDto.FromDictionaryResponse(definition, 0, isLearned, definition.Vietnamese);

                _logger.LogInformation("Successfully retrieved definition for word: {Word}", word);
                return Ok(response);
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

        /// <summary>
        /// Kiểm tra một từ đã học hay chưa
        /// </summary>
        [HttpGet("islearned/{word}")]
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

        /// <summary>
        /// Đánh dấu một từ là đã học cho user hiện tại
        /// </summary>
        [HttpPost("learned/{word}")]
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

        /// <summary>
        /// Lấy userId từ claim của user hiện tại
        /// </summary>
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
