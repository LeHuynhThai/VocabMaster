using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Api.Contracts.Common;
using VocabMaster.Api.Contracts.Vocabulary;
using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;

namespace VocabMaster.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LearnedWordController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IVocabularyService _vocabularyService;

        public LearnedWordController(ICurrentUserService currentUserService, IVocabularyService vocabularyService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _vocabularyService = vocabularyService;
        }

        [HttpGet("learned-word")]
        public async Task<IActionResult> GetAll()
        {
            var userId = _currentUserService.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new ErrorResponse
                {
                    Error = "auth_error",
                    Message = "Không thể xác thực người dùng"
                });
            }

            try
            {
                var items = await _vocabularyService.GetLearnedWords(userId.Value);
                var result = items.Select(ToLearnedWordResponse).ToList();

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "server_error",
                    Message = "Đã xảy ra lỗi khi lấy danh sách từ đã học"
                });
            }
        }

        [HttpDelete("learned-word/{id}")]
        public async Task<IActionResult> RemoveLearnedWord(int id)
        {
            var userId = _currentUserService.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new ErrorResponse
                {
                    Error = "auth_error",
                    Message = "Không thể xác thực người dùng"
                });
            }

            try
            {
                var result = await _vocabularyService.RemoveLearnedWord(id, userId.Value);

                if (result)
                {
                    return Ok(new OperationResultResponse
                    {
                        Success = true,
                        Message = "Xóa từ vựng đã học thành công"
                    });
                }

                return NotFound(new OperationResultResponse
                {
                    Success = false,
                    Message = "Không tìm thấy từ vựng đã học"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "server_error",
                    Message = "Đã xảy ra lỗi khi xóa từ vựng đã học"
                });
            }
        }

        private static LearnedWordResponse ToLearnedWordResponse(LearnedWord learnedWord)
        {
            return new LearnedWordResponse
            {
                Id = learnedWord.Id,
                Word = learnedWord.Word,
                LearnedAt = learnedWord.LearnedAt
            };
        }
    }
}