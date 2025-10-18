using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.DTOs;
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
        public async Task<IActionResult> CrawFromApi([FromBody] CrawlVocabularyRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Word))
                {
                    return BadRequest(new { message = "Word is required" });
                }

                var result = await _adminDashBoardService.CrawFromApi(request.Word);

                var response = new AdminVocabularyResponseDto
                {
                    Id = result.Id,
                    Word = result.Word,
                    Vietnamese = result.Vietnamese,
                    MeaningsJson = result.MeaningsJson,
                    PronunciationsJson = result.PhoneticsJson
                };

                return Ok(new { message = "Đã lấy dữ liệu và lưu", data = response });
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

                var response = vocabularies.Select(v => new AdminVocabularyResponseDto
                {
                    Id = v.Id,
                    Word = v.Word,
                    Vietnamese = v.Vietnamese,
                    MeaningsJson = v.MeaningsJson,
                    PronunciationsJson = v.PhoneticsJson
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
        public async Task<IActionResult> AddVocabulary([FromBody] AddVocabularyRequestDto request)
        {
            try
            {
                var vocabulary = new Vocabulary
                {
                    Word = request.Word,
                    Vietnamese = request.Vietnamese,
                    MeaningsJson = request.MeaningsJson,
                    PhoneticsJson = request.PronunciationsJson
                };

                var result = await _adminDashBoardService.AddVocabulary(vocabulary);

                var response = new AdminVocabularyResponseDto
                {
                    Id = result.Id,
                    Word = result.Word,
                    Vietnamese = result.Vietnamese,
                    MeaningsJson = result.MeaningsJson,
                    PronunciationsJson = result.PhoneticsJson
                };

                return Ok(new { 
                    message = "Từ vựng đã được thêm thành công",
                    data = response 
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
