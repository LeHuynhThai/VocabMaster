using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VocabMaster.Api.Contracts.Common;
using VocabMaster.Api.Contracts.Vocabulary;
using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;

namespace VocabMaster.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WordGeneratorController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IVocabularyService _vocabularyService;
        private readonly IMemoryCache _cache;
        private const string RandomWordCacheKey = "RandomWord_";

        public WordGeneratorController(ICurrentUserService currentUserService, IVocabularyService vocabularyService, IMemoryCache cache)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _vocabularyService = vocabularyService;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [HttpGet("word-detail/{word}")]
        public async Task<IActionResult> GetWordDetail(string word)
        {
            try
            {
                var vocabulary = await _vocabularyService.GetVocabularyByWord(word);
                if (vocabulary == null)
                {
                    return NotFound(new MessageResponse { Message = "Không tìm thấy từ vựng" });
                }

                return Ok(ToVocabularySummaryResponse(vocabulary));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "server_error",
                    Message = "Lỗi server"
                });
            }
        }

        [HttpGet("random-word")]
        public async Task<IActionResult> GetRandomWordExcludeLearned()
        {
            try
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

                string cacheKey = $"{RandomWordCacheKey}{userId}";
                _cache.Remove(cacheKey);

                var randomWord = await _vocabularyService.GetRandomWord(userId.Value);

                if (randomWord == null)
                {
                    return Ok(new AllLearnedVocabularyResponse
                    {
                        AllLearned = true,
                        Message = "Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống."
                    });
                }

                return Ok(ToVocabularySummaryResponse(randomWord));

            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "server_error",
                    Message = "Đã xảy ra lỗi khi lấy từ ngẫu nhiên chưa học"
                });
            }
        }

        [HttpPost("learned-word")]
        public async Task<IActionResult> AddLearnedWord([FromBody] AddLearnedWordRequest request)
        {
            try
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

                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Word))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "validation_error",
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var result = await _vocabularyService.AddLearnedWord(request.Word, userId.Value);

                return Ok(new DataResponse<LearnedWordResponse>
                {
                    Success = true,
                    Message = "Đã lưu từ vựng thành công",
                    Data = new LearnedWordResponse
                    {
                        Id = result.Id,
                        Word = result.Word,
                        LearnedAt = result.LearnedAt
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "server_error",
                    Message = "Đã xảy ra lỗi khi lưu từ vựng"
                });
            }
        }

        private static VocabularySummaryResponse ToVocabularySummaryResponse(Vocabulary vocabulary)
        {
            return new VocabularySummaryResponse
            {
                Id = vocabulary.Id,
                Word = vocabulary.Word,
                Vietnamese = vocabulary.Vietnamese
            };
        }
    }
}
