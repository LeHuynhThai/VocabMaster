using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Quiz;

namespace VocabMaster.API.Controllers
{
    // Controller quản lý các API liên quan đến trắc nghiệm (quiz) cho người dùng
    // Bao gồm lấy câu hỏi, kiểm tra đáp án, lấy thống kê, phân trang, ...
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {
        // Service lấy câu hỏi trắc nghiệm
        private readonly IQuizQuestionService _quizQuestionService;
        // Service kiểm tra đáp án
        private readonly IQuizAnswerService _quizAnswerService;
        // Service quản lý tiến trình làm quiz của user
        private readonly IQuizProgressService _quizProgressService;
        // Repository thao tác trực tiếp với bảng câu hỏi
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        // Ghi log cho controller
        private readonly ILogger<QuizController> _logger;

        // Hàm khởi tạo controller, inject các service cần thiết
        public QuizController(
            IQuizQuestionService quizQuestionService,
            IQuizAnswerService quizAnswerService,
            IQuizProgressService quizProgressService,
            IQuizQuestionRepo quizQuestionRepo,
            ILogger<QuizController> logger)
        {
            _quizQuestionService = quizQuestionService ?? throw new ArgumentNullException(nameof(quizQuestionService));
            _quizAnswerService = quizAnswerService ?? throw new ArgumentNullException(nameof(quizAnswerService));
            _quizProgressService = quizProgressService ?? throw new ArgumentNullException(nameof(quizProgressService));
            _quizQuestionRepo = quizQuestionRepo ?? throw new ArgumentNullException(nameof(quizQuestionRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Lấy một câu hỏi trắc nghiệm ngẫu nhiên
        /// </summary>
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomQuestion()
        {
            try
            {
                _logger.LogInformation("Getting random quiz question");

                // Lấy câu hỏi ngẫu nhiên từ service
                var question = await _quizQuestionService.GetRandomQuestion();

                if (question == null)
                {
                    _logger.LogWarning("No quiz questions available");
                    return NotFound(new
                    {
                        error = "question_not_found",
                        message = "Không tìm thấy câu hỏi nào. Vui lòng thêm câu hỏi vào hệ thống."
                    });
                }

                _logger.LogInformation("Successfully retrieved random question: {QuestionId}", question.Id);
                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting random quiz question: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi tải câu hỏi. Vui lòng thử lại sau.",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy một câu hỏi trắc nghiệm mà user chưa hoàn thành (nếu có)
        /// Nếu user đã làm hết thì trả về thông báo và thống kê
        /// </summary>
        [HttpGet("uncompleted")]
        public async Task<IActionResult> GetRandomUncompletedQuestion()
        {
            try
            {
                var userId = GetUserIdSafe(); // Lấy userId từ claim (có thể null)
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, returning random question instead");
                    // Nếu không xác định được user thì trả về câu hỏi ngẫu nhiên
                    var question = await _quizQuestionService.GetRandomQuestion();

                    if (question == null)
                    {
                        return NotFound(new
                        {
                            error = "question_not_found",
                            message = "Không tìm thấy câu hỏi nào"
                        });
                    }

                    return Ok(question);
                }

                // Lấy câu hỏi chưa hoàn thành của user
                var uncompletedQuestion = await _quizQuestionService.GetRandomUncompletedQuestion(userId.Value);

                if (uncompletedQuestion == null)
                {
                    _logger.LogInformation("User {UserId} has completed all quiz questions", userId);
                    
                    // Lấy thống kê quiz để trả về kèm thông báo
                    var stats = await _quizProgressService.GetQuizStatistics(userId.Value);
                    
                    return Ok(new
                    {
                        allCompleted = true,
                        message = "Chúc mừng! Bạn đã hoàn thành tất cả các câu hỏi trắc nghiệm.",
                        stats = stats
                    });
                }

                return Ok(uncompletedQuestion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random uncompleted quiz question");
                return StatusCode(500, new
                {
                    error = "question_error",
                    message = "Đã xảy ra lỗi khi tải câu hỏi",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Kiểm tra đáp án của một câu hỏi trắc nghiệm
        /// </summary>
        [HttpPost("check")]
        public async Task<IActionResult> CheckAnswer([FromBody] QuizAnswerDto quizAnswerDto)
        {
            try
            {
                if (quizAnswerDto == null)
                {
                    // Kiểm tra dữ liệu đầu vào
                    return BadRequest(new
                    {
                        error = "invalid_input",
                        message = "Dữ liệu câu trả lời không hợp lệ"
                    });
                }

                // Kiểm tra đáp án qua service
                var result = await _quizAnswerService.CheckAnswer(quizAnswerDto.QuestionId, quizAnswerDto.SelectedAnswer);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiz answer");
                return StatusCode(500, new
                {
                    error = "check_error",
                    message = "Đã xảy ra lỗi khi kiểm tra câu trả lời",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Kiểm tra đáp án và đánh dấu đã hoàn thành cho user (nếu xác định được user)
        /// </summary>
        [HttpPost("check-complete")]
        public async Task<IActionResult> CheckAnswerAndMarkCompleted([FromBody] QuizAnswerDto quizAnswerDto)
        {
            try
            {
                if (quizAnswerDto == null)
                {
                    return BadRequest(new
                    {
                        error = "invalid_input",
                        message = "Dữ liệu câu trả lời không hợp lệ"
                    });
                }

                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, checking answer without marking completed");
                    // Nếu không xác định được user thì chỉ kiểm tra đáp án, không đánh dấu hoàn thành
                    var resultUserId = await _quizAnswerService.CheckAnswer(quizAnswerDto.QuestionId, quizAnswerDto.SelectedAnswer);
                    return Ok(resultUserId);
                }

                // Kiểm tra đáp án và đánh dấu đã hoàn thành cho user
                var result = await _quizAnswerService.CheckAnswerAndMarkCompleted(
                    quizAnswerDto.QuestionId,
                    quizAnswerDto.SelectedAnswer,
                    userId.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and marking quiz answer");
                return StatusCode(500, new
                {
                    error = "check_error",
                    message = "Đã xảy ra lỗi khi kiểm tra câu trả lời",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách các quiz đã hoàn thành của user
        /// </summary>
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedQuizzes()
        {
            try
            {
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, returning empty completed list");
                    return Ok(new List<CompletedQuizDto>());
                }

                // Lấy danh sách quiz đã hoàn thành qua service
                var completedQuizzes = await _quizProgressService.GetCompletedQuizzes(userId.Value);
                return Ok(completedQuizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed quizzes");
                return StatusCode(500, new
                {
                    error = "completed_error",
                    message = "Đã xảy ra lỗi khi tải danh sách câu hỏi đã hoàn thành",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách các quiz user đã làm đúng
        /// </summary>
        [HttpGet("correct")]
        public async Task<IActionResult> GetCompleteQuizz()
        {
            try
            {
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, returning empty correct list");
                    return Ok(new List<CompletedQuizDto>());
                }

                // Lấy danh sách quiz đã làm đúng qua service
                var correctQuizzes = await _quizProgressService.GetCompleteQuizz(userId.Value);
                return Ok(correctQuizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting correct quizzes");
                return StatusCode(500, new
                {
                    error = "correct_quizzes_error",
                    message = "Đã xảy ra lỗi khi tải danh sách câu hỏi đã làm đúng",
                    details = ex.Message
                });
            }
        }
        
        /// <summary>
        /// Lấy danh sách quiz user đã làm đúng (có phân trang)
        /// </summary>
        [HttpGet("correct/paginated")]
        public async Task<IActionResult> GetPaginatedCorrectQuizzes([FromQuery] int pageNumber = 1)
        {
            try
            {
                // Kiểm tra tham số phân trang
                if (pageNumber < 1) pageNumber = 1;
                // Sử dụng page size mặc định là 10
                const int pageSize = 10;
                
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, returning empty paginated correct list");
                    return Ok(new PaginatedResponseDto<CompletedQuizDto>
                    {
                        Items = new List<CompletedQuizDto>(),
                        PageInfo = new PageInfoDto
                        {
                            CurrentPage = pageNumber,
                            PageSize = pageSize,
                            TotalItems = 0,
                            TotalPages = 0
                        }
                    });
                }
                
                _logger.LogInformation("Getting paginated correct quizzes for user {UserId}, page {Page}", 
                    userId, pageNumber);
                
                // Lấy dữ liệu phân trang từ service
                var (items, totalCount, totalPages) = await _quizProgressService.GetPaginatedCorrectQuizzes(userId.Value, pageNumber, pageSize);
                
                _logger.LogInformation("Retrieved {Count} items, total {Total}, pages {Pages}", 
                    items.Count, totalCount, totalPages);
                
                // Tạo response phân trang
                var response = new PaginatedResponseDto<CompletedQuizDto>
                {
                    Items = items ?? new List<CompletedQuizDto>(),
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
                _logger.LogError(ex, "Error getting paginated correct quizzes: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "paginated_correct_error",
                    message = "Đã xảy ra lỗi khi tải danh sách câu hỏi đã làm đúng theo trang",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thống kê quiz cho user hiện tại
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetQuizStatistics()
        {
            try
            {
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, returning default stats");
                    
                    // Lấy tổng số câu hỏi quiz
                    int totalQuestions;
                    try
                    {
                        // Gọi trực tiếp repository để đếm số lượng câu hỏi
                        totalQuestions = await _quizQuestionRepo.CountQuizQuestions();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error counting quiz questions");
                        totalQuestions = 0;
                    }
                    
                    // Trả về thống kê mặc định nếu không xác định được user
                    return Ok(new QuizStatsDto
                    {
                        TotalQuestions = totalQuestions,
                        CompletedQuestions = 0,
                        CorrectAnswers = 0,
                        CompletionPercentage = 0,
                        CorrectPercentage = 0
                    });
                }

                // Lấy thống kê quiz qua service
                var stats = await _quizProgressService.GetQuizStatistics(userId.Value);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz statistics");
                return StatusCode(500, new
                {
                    error = "stats_error",
                    message = "Đã xảy ra lỗi khi tải thống kê trắc nghiệm",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy userId từ claim của user hiện tại (có thể null nếu không xác thực)
        /// </summary>
        private int? GetUserIdSafe()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                              ?? User.Claims.FirstOrDefault(c => c.Type == "userId")
                              ?? User.Claims.FirstOrDefault(c => c.Type == "id")
                              ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                _logger.LogWarning("User ID claim not found or not valid. Available claims: {Claims}",
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from claims");
                return null;
            }
        }

        /// <summary>
        /// Lấy userId hiện tại, nếu không có thì throw exception
        /// </summary>
        private int GetCurrentUserId()
        {
            var userId = GetUserIdSafe();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            return userId.Value;
        }
    }
}