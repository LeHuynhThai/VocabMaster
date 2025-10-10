using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Services.Implementation;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Dictionary;
using VocabMaster.Core.Interfaces.Services.Vocabulary;
using static Services.Implementation.DictionaryCacheService;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WordGeneratorController : ControllerBase
    {
        private readonly RandomWordService _randomWordService;
        private readonly DictionaryLookupService _dictionaryLookupService;
        private readonly ILearnedWordService _learnedWordService;
        private readonly WordStatusService _wordStatusService;
        private readonly IMemoryCache _cache;
        private const string RandomWordCacheKey = "RandomWord_";
        private const string LookupCacheKey = "Lookup_";
        private const int CacheExpirationMinutes = 30;

        public WordGeneratorController(
            RandomWordService randomWordService,
            DictionaryLookupService dictionaryLookupService,
            ILearnedWordService learnedWordService,
            WordStatusService wordStatusService,
            IMemoryCache cache = null)
        {
            _randomWordService = randomWordService ?? throw new ArgumentNullException(nameof(randomWordService));
            _dictionaryLookupService = dictionaryLookupService ?? throw new ArgumentNullException(nameof(dictionaryLookupService));
            _learnedWordService = learnedWordService ?? throw new ArgumentNullException(nameof(learnedWordService));
            _wordStatusService = wordStatusService ?? throw new ArgumentNullException(nameof(wordStatusService));
            _cache = cache;
        }

        [HttpGet("getrandomword")]
        public async Task<IActionResult> GetRandomWord()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
                }

                string cacheKey = $"{RandomWordCacheKey}{userId}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out VocabularyResponseDto cachedWord))
                {
                    return Ok(cachedWord);
                }

                var randomWord = await _randomWordService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    return Ok(new { 
                        allLearned = true, 
                        message = "Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống."
                    });
                }

                bool isLearned = await _wordStatusService.IsWordLearned(userId, randomWord.Word);

                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned, randomWord.Vietnamese);

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
                return StatusCode(500, new
                {
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi lấy từ ngẫu nhiên",
                    details = ex.Message
                });
            }
        }

        [HttpGet("newrandomword")]
        public async Task<IActionResult> GetRandomWordExcludeLearned()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
                }

                string cacheKey = $"{RandomWordCacheKey}{userId}";
                if (_cache != null)
                {
                    _cache.Remove(cacheKey);
                }

                var randomWord = await _randomWordService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    return Ok(new { 
                        allLearned = true, 
                        message = "Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống."
                    });
                }

                bool isLearned = await _wordStatusService.IsWordLearned(userId, randomWord.Word);

                var response = VocabularyResponseDto.FromDictionaryResponse(randomWord, 0, isLearned, randomWord.Vietnamese);

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

                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
                }

                var definition = await _dictionaryLookupService.GetWordDefinitionFromDatabase(word);

                if (definition == null)
                {
                    return NotFound(new
                    {
                        error = "word_not_found",
                        message = $"Không tìm thấy từ vựng trong cơ sở dữ liệu, vui lòng tìm kiếm từ khác"
                    });
                }

                bool isLearned = await _wordStatusService.IsWordLearned(userId, word);
                
                var response = VocabularyResponseDto.FromDictionaryResponse(definition, 0, isLearned, definition.Vietnamese);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "lookup_error",
                    message = $"Đã xảy ra lỗi khi tra cứu định nghĩa cho từ: {word}",
                    details = ex.Message
                });
            }
        }

        [HttpGet("islearned/{word}")]
        public async Task<IActionResult> IsLearned(string word)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
            }

            try
            {
                bool isLearned = await _wordStatusService.IsWordLearned(userId, word);

                return Ok(new { isLearned });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "check_error",
                    message = $"Đã xảy ra lỗi khi kiểm tra từ {word} đã học hay chưa",
                    details = ex.Message
                });
            }
        }

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
                return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
            }

            try
            {
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

            return 0;
        }
    }
}
