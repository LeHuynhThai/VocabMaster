using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Dictionary;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.API.Controllers
{
    // Controller quản lý các API liên quan đến từ đã học của người dùng
    // Sử dụng các service để thao tác với dữ liệu từ đã học, tra cứu từ điển, và caching
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LearnedWordController : ControllerBase
    {
        // Service xử lý logic từ đã học
        private readonly ILearnedWordService _learnedWordService;
        // Service tra cứu thông tin từ điển
        private readonly IDictionaryLookupService _dictionaryLookupService;
        // Ghi log cho controller
        private readonly ILogger<LearnedWordController> _logger;
        // Bộ nhớ đệm (cache) trong bộ nhớ
        private readonly IMemoryCache _cache;
        // AutoMapper để chuyển đổi giữa entity và DTO
        private readonly IMapper _mapper;
        // Key dùng cho cache danh sách từ đã học
        private const string LearnedWordsListCacheKey = "LearnedWordsList_";
        // Thời gian hết hạn cache (phút)
        private const int CacheExpirationMinutes = 5;

        // Hàm khởi tạo controller, inject các service cần thiết
        public LearnedWordController(
            ILearnedWordService learnedWordService,
            IDictionaryLookupService dictionaryLookupService,
            ILogger<LearnedWordController> logger,
            IMapper mapper,
            IMemoryCache cache = null)
        {
            _learnedWordService = learnedWordService ?? throw new ArgumentNullException(nameof(learnedWordService));
            _dictionaryLookupService = dictionaryLookupService ?? throw new ArgumentNullException(nameof(dictionaryLookupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách từ đã học của người dùng hiện tại
        /// Có sử dụng cache để tăng hiệu năng, có thể bỏ qua cache bằng query ?t=1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLearnedWords()
        {
            try
            {
                var userId = GetUserIdFromClaims(); // Lấy userId từ claim
                if (userId <= 0)
                {
                    // Nếu không xác thực được user
                    return Unauthorized(new
                    {
                        error = "auth_error",
                        message = "Không thể xác thực người dùng"
                    });
                }

                bool skipCache = Request.Query.ContainsKey("t"); // Nếu có query t thì bỏ qua cache

                string cacheKey = $"{LearnedWordsListCacheKey}{userId}";
                // Nếu không bỏ qua cache và cache tồn tại thì trả về dữ liệu từ cache
                if (!skipCache && _cache != null && _cache.TryGetValue(cacheKey, out List<LearnedWordDto> cachedWords))
                {
                    _logger.LogInformation("Retrieved learned words from cache for user {UserId}", userId);
                    return Ok(cachedWords);
                }

                _logger.LogInformation("Getting learned words for user {UserId}", userId);
                // Lấy danh sách từ đã học từ service
                var learnedWords = await _learnedWordService.GetUserLearnedVocabularies(userId);

                if (learnedWords == null)
                {
                    _logger.LogInformation("No learned words found for user {UserId}", userId);
                    return Ok(new List<LearnedWordDto>());
                }

                // Map sang DTO để trả về client
                var response = _mapper.Map<List<LearnedWordDto>>(learnedWords);

                // Lưu vào cache nếu cần
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
                _logger.LogError(ex, "Error loading learned words");
                // Trả về lỗi chi tiết nếu có exception
                return StatusCode(500, new
                {
                    error = "load_error",
                    message = "Đã xảy ra lỗi khi tải danh sách từ đã học",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết một từ đã học theo id (của user hiện tại)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLearnedWord(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims(); // Lấy userId từ claim
                if (userId <= 0)
                {
                    return Unauthorized(new
                    {
                        error = "auth_error",
                        message = "Không thể xác thực người dùng"
                    });
                }

                // Lấy từ đã học theo id
                var learnedWord = await _learnedWordService.GetLearnedWordById(userId, id);
                if (learnedWord == null)
                {
                    return NotFound(new
                    {
                        error = "word_not_found",
                        message = $"Không tìm thấy từ đã học với ID: {id}"
                    });
                }

                // Tra cứu chi tiết từ điển cho từ đã học
                var wordDetails = await _dictionaryLookupService.GetWordDefinition(learnedWord.Word);
                if (wordDetails == null)
                {
                    // Nếu không tra được thì chỉ trả về thông tin cơ bản
                    return Ok(_mapper.Map<LearnedWordDto>(learnedWord));
                }

                // Tạo response chi tiết từ kết quả tra cứu
                var response = VocabularyResponseDto.FromDictionaryResponse(wordDetails, learnedWord.Id, true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned word details");
                return StatusCode(500, new
                {
                    error = "details_error",
                    message = $"Đã xảy ra lỗi khi lấy chi tiết từ đã học với ID: {id}",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách từ đã học có phân trang
        /// </summary>
        /// <param name="pageNumber">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng từ mỗi trang (tối đa 50)</param>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedLearnedWords([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Kiểm tra và giới hạn tham số phân trang
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 50) pageSize = 50; // Giới hạn tối đa 50 từ/trang
                
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new
                    {
                        error = "auth_error",
                        message = "Không thể xác thực người dùng"
                    });
                }
                
                _logger.LogInformation("Getting paginated learned words for user {UserId}, page {Page}, size {Size}", 
                    userId, pageNumber, pageSize);
                
                // Lấy dữ liệu phân trang từ service
                var (items, totalCount, totalPages) = await _learnedWordService.GetPaginatedLearnedWords(userId, pageNumber, pageSize);
                
                // Tạo response phân trang
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
                _logger.LogError(ex, "Error getting paginated learned words");
                return StatusCode(500, new
                {
                    error = "pagination_error",
                    message = "Đã xảy ra lỗi khi tải danh sách từ đã học theo trang",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Thêm một từ vào danh sách từ đã học của user hiện tại
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddLearnedWord([FromBody] AddLearnedWordDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Word))
            {
                // Kiểm tra dữ liệu đầu vào
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
                _logger.LogInformation("Adding word '{Word}' to learned list for user {UserId}", request.Word, userId);
                // Đánh dấu từ đã học qua service
                var result = await _learnedWordService.MarkWordAsLearned(userId, request.Word.Trim());

                if (result.Success)
                {
                    InvalidateCache(userId); // Xóa cache để lần sau lấy lại dữ liệu mới

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
                _logger.LogError(ex, "Error marking word as learned: {Word}", request.Word);
                return StatusCode(500, new
                {
                    error = "add_error",
                    message = $"Đã xảy ra lỗi khi thêm từ {request.Word} vào danh sách từ đã học",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Xóa một từ đã học khỏi danh sách của user hiện tại
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearnedWord(int id)
        {
            if (id <= 0)
            {
                // Kiểm tra id hợp lệ
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
                _logger.LogInformation("Removing learned word with ID {WordId} for user {UserId}", id, userId);
                // Xóa từ đã học qua service
                var result = await _learnedWordService.RemoveLearnedWordById(userId, id);

                if (result)
                {
                    InvalidateCache(userId); // Xóa cache để đồng bộ dữ liệu
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
                _logger.LogError(ex, "Error removing learned word: {Id}", id);
                return StatusCode(500, new
                {
                    error = "delete_error",
                    message = $"Đã xảy ra lỗi khi xóa từ đã học với ID: {id}",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy userId từ claim của user hiện tại
        /// </summary>
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) ??
                              User.Claims.FirstOrDefault(c => c.Type == "UserId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            _logger.LogWarning("UserId not found in claims or could not be parsed");
            return 0;
        }

        /// <summary>
        /// Xóa cache danh sách từ đã học của user (khi thêm/xóa từ)
        /// </summary>
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
