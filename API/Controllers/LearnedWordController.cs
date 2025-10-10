using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Services.Implementation;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Dictionary;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LearnedWordController : ControllerBase
    { 
        private readonly ILearnedWordService _learnedWordService;
        private readonly DictionaryLookupService _dictionaryLookupService;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private const string LearnedWordsListCacheKey = "LearnedWordsList_";
        private const int CacheExpirationMinutes = 5;

        public LearnedWordController(
            ILearnedWordService learnedWordService,
            DictionaryLookupService dictionaryLookupService,
            IMapper mapper,
            IMemoryCache cache = null)
        {
            _learnedWordService = learnedWordService ?? throw new ArgumentNullException(nameof(learnedWordService));
            _dictionaryLookupService = dictionaryLookupService ?? throw new ArgumentNullException(nameof(dictionaryLookupService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetLearnedWords()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new
                    {
                        error = "auth_error",
                        message = "Không thể xác thực người dùng"
                    });
                }

                bool skipCache = Request.Query.ContainsKey("t");

                string cacheKey = $"{LearnedWordsListCacheKey}{userId}";
                if (!skipCache && _cache != null && _cache.TryGetValue(cacheKey, out List<LearnedWordDto> cachedWords))
                {
                    return Ok(cachedWords);
                }

                var learnedWords = await _learnedWordService.GetUserLearnedVocabularies(userId);

                if (learnedWords == null)
                {
                    return Ok(new List<LearnedWordDto>());
                }

                var response = _mapper.Map<List<LearnedWordDto>>(learnedWords);

                if (!skipCache && _cache != null)
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
                    error = "load_error",
                    message = "Đã xảy ra lỗi khi tải danh sách từ đã học",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLearnedWord(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new
                    {
                        error = "auth_error",
                        message = "Không thể xác thực người dùng"
                    });
                }

                var learnedWord = await _learnedWordService.GetLearnedWordById(userId, id);
                if (learnedWord == null)
                {
                    return NotFound(new
                    {
                        error = "word_not_found",
                        message = $"Không tìm thấy từ đã học với ID: {id}"
                    });
                }

                var wordDetails = await _dictionaryLookupService.GetWordDefinition(learnedWord.Word);
                if (wordDetails == null)
                {
                    return Ok(_mapper.Map<LearnedWordDto>(learnedWord));
                }

                var response = VocabularyResponseDto.FromDictionaryResponse(wordDetails, learnedWord.Id, true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "details_error",
                    message = $"Đã xảy ra lỗi khi lấy chi tiết từ đã học với ID: {id}",
                    details = ex.Message
                });
            }
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedLearnedWords([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 50) pageSize = 50;
                
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new
                    {
                        error = "auth_error",
                        message = "Không thể xác thực người dùng"
                    });
                }
                
                
                var (items, totalCount, totalPages) = await _learnedWordService.GetPaginatedLearnedWords(userId, pageNumber, pageSize);
                
                var response = new PaginatedResponseDto<LearnedWordDto>
                {
                    Items = items,
                    PageInfo = new PageInfoDto
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalItems = totalCount,
                        TotalPages = totalPages
                    }
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "pagination_error",
                    message = "Đã xảy ra lỗi khi tải danh sách từ đã học theo trang",
                    details = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLearnedWord([FromBody] AddLearnedWordDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Word))
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
                return Unauthorized(new
                {
                    error = "auth_error",
                    message = "Không thể xác thực người dùng"
                });
            }

            try
            {
                var result = await _learnedWordService.MarkWordAsLearned(userId, request.Word.Trim());

                if (result.Success)
                {
                    InvalidateCache(userId);

                    return Ok(new
                    {
                        id = result.Data?.Id ?? 0,
                        word = request.Word.Trim(),
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        error = "mark_error",
                        message = result.ErrorMessage ?? "Không thể đánh dấu từ đã học. Vui lòng thử lại."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "add_error",
                    message = $"Đã xảy ra lỗi khi thêm từ {request.Word} vào danh sách từ đã học",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearnedWord(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    error = "invalid_id",
                    message = "ID từ không hợp lệ"
                });
            }

            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                return Unauthorized(new
                {
                    error = "auth_error",
                    message = "Không thể xác thực người dùng"
                });
            }

            try
            {
                var result = await _learnedWordService.RemoveLearnedWordById(userId, id);

                if (result)
                {
                    InvalidateCache(userId);
                    return Ok(new { success = true });
                }
                else
                {
                    return NotFound(new
                    {
                        error = "word_not_found",
                        message = $"Không tìm thấy từ đã học với ID: {id} hoặc bạn không có quyền xóa nó"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "delete_error",
                    message = $"Đã xảy ra lỗi khi xóa từ đã học với ID: {id}",
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

        private void InvalidateCache(int userId)
        {
            if (_cache != null)
            {
                string cacheKey = $"{LearnedWordsListCacheKey}{userId}";
                _cache.Remove(cacheKey);
            }
        }
    }
}
