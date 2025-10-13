using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Repository.DTOs;
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
                    return Ok(new { 
                        allLearned = true, 
                        message = "Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống."
                    });
                }
                return Ok(randomWord);
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
