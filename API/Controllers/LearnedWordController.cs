using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Repository.DTOs;
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

                var result = items.Select(x => new LearnedWordDto
                {
                    Id = x.Id,
                    Word = x.Word,
                    LearnedAt = x.LearnedAt
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