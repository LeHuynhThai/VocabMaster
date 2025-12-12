using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LearnedWordController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;

        public LearnedWordController(IVocabularyService vocabularyService)
        {
            _vocabularyService = vocabularyService;
        }

        [HttpGet("learned-word")]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserIdFromClaims();
            try
            {
                var items = await _vocabularyService.GetLearnedWords(userId);

                var result = items.Select(x => new
                {
                    id = x.Id,
                    word = x.Word,
                    learnedAt = x.LearnedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi lấy danh sách từ đã học",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("learned-word/{id}")]
        public async Task<IActionResult> RemoveLearnedWord(int id)
        {
            try
            {
                var result = await _vocabularyService.RemoveLearnedWord(id);
                
                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Xóa từ vựng đã học thành công"
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy từ vựng đã học"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi xóa từ vựng đã học",
                    details = ex.Message
                });
            }
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                              ?? User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}