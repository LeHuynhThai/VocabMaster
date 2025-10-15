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
                    PronunciationsJson = request.PronunciationsJson
                };

                var result = await _adminDashBoardService.AddVocabulary(vocabulary);

                var response = new VocabularyResponseDto
                {
                    Id = result.Id,
                    Word = result.Word,
                    Vietnamese = result.Vietnamese,
                    MeaningsJson = result.MeaningsJson,
                    PronunciationsJson = result.PronunciationsJson
                };

                return Ok(new { 
                    message = "Từ vựng đã được thêm thành công",
                    data = response 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
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
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
