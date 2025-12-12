using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Service.Interfaces;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDashBoardController : ControllerBase
    {
        private readonly IAdminDashBoardService _adminDashBoardService;

        public AdminDashBoardController(IAdminDashBoardService adminDashBoardService)
        {
            _adminDashBoardService = adminDashBoardService;
        }

        [HttpPost("vocabulary/crawl")]
        public async Task<IActionResult> CrawFromApi([FromBody] Dictionary<string, string> request)
        {
            try
            {
                if (request == null || !request.TryGetValue("word", out var word) || string.IsNullOrWhiteSpace(word))
                {
                    return BadRequest(new { message = "Word is required" });
                }

                var result = await _adminDashBoardService.CrawFromApi(word);

                return Ok(new
                {
                    message = "Đã lấy dữ liệu và lưu",
                    data = new
                    {
                        id = result.Id,
                        word = result.Word,
                        vietnamese = result.Vietnamese
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crawling word: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("vocabulary")]
        public async Task<IActionResult> GetVocabularies()
        {
            try
            {
                var vocabularies = await _adminDashBoardService.GetVocabularies();

                var response = vocabularies.Select(v => new
                {
                    id = v.Id,
                    word = v.Word,
                    vietnamese = v.Vietnamese
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting vocabularies: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("vocabulary")]
        public async Task<IActionResult> AddVocabulary([FromBody] Vocabulary request)
        {
            try
            {
                var result = await _adminDashBoardService.AddVocabulary(request);

                return Ok(new {
                    message = "Từ vựng đã được thêm thành công",
                    data = new
                    {
                        id = result.Id,
                        word = result.Word,
                        vietnamese = result.Vietnamese
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding vocabulary: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("vocabulary/{vocabularyId}")]
        public async Task<IActionResult> DeleteVocabulary(int vocabularyId)
        {
            try
            {
                var success = await _adminDashBoardService.DeleteVocabulary(vocabularyId);

                if (success)
                {
                    return Ok(new { message = "Từ vựng đã được xóa thành công" });
                }
                else
                {
                    return NotFound(new { message = "Không tìm thấy từ vựng" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting vocabulary: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
