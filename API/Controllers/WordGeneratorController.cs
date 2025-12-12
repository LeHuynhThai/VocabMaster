using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Repository.Entities;
using Service.Interfaces;
using System.Security.Claims;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WordGeneratorController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;
        private readonly IMemoryCache _cache;
        private const string RandomWordCacheKey = "RandomWord_";

        public WordGeneratorController(IVocabularyService vocabularyService, IMemoryCache cache = null)
        {
            _vocabularyService = vocabularyService;
            _cache = cache;
        }

        private static T? SafeDeserialize<T>(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return default;
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        [HttpGet("word-detail/{word}")]
        public async Task<IActionResult> GetWordDetail(string word)
        {
            try
            {
                var vocabulary = await _vocabularyService.GetVocabularyByWord(word);
                if (vocabulary == null)
                {
                    return NotFound(new { message = "Không tìm thấy từ vựng" });
                }

                return Ok(new
                {
                    id = vocabulary.Id,
                    word = vocabulary.Word,
                    vietnamese = vocabulary.Vietnamese
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", details = ex.Message });
            }
        }

        [HttpGet("random-word")]
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

                var randomWord = await _vocabularyService.GetRandomWord(userId);

                if (randomWord == null)
                {
                    return Ok(new
                    {
                        allLearned = true,
                        message = "Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống."
                    });
                }

                return Ok(new
                {
                    id = randomWord.Id,
                    word = randomWord.Word,
                    vietnamese = randomWord.Vietnamese
                });

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

        [HttpPost("learned-word")]
        public async Task<IActionResult> AddLearnedWord([FromBody] Dictionary<string, string> dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new { error = "auth_error", message = "Không thể xác thực người dùng" });
                }

                if (dto == null || !dto.TryGetValue("word", out var word) || string.IsNullOrWhiteSpace(word))
                {
                    return BadRequest(new { error = "validation_error", message = "Dữ liệu không hợp lệ" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "validation_error", message = "Dữ liệu không hợp lệ", details = ModelState });
                }

                var learnedWord = new LearnedWord
                {
                    Word = word,
                    UserId = userId,
                    LearnedAt = DateTime.UtcNow
                };

                var result = await _vocabularyService.AddLearnedWord(learnedWord);

                return Ok(new
                {
                    success = true,
                    message = "Đã lưu từ vựng thành công",
                    data = new
                    {
                        id = result.Id,
                        word = result.Word,
                        learnedAt = result.LearnedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi lưu từ vựng",
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
