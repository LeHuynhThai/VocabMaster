using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Quiz;

namespace VocabMaster.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizQuestionService _quizQuestionService;
        private readonly IQuizAnswerService _quizAnswerService;
        private readonly IQuizProgressService _quizProgressService;
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly ILogger<QuizController> _logger;

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

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomQuestion()
        {
            try
            {
                _logger.LogInformation("Getting random quiz question");

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

        [HttpGet("uncompleted")]
        public async Task<IActionResult> GetRandomUncompletedQuestion()
        {
            try
            {
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, returning random question instead");
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

                var uncompletedQuestion = await _quizQuestionService.GetRandomUncompletedQuestion(userId.Value);

                if (uncompletedQuestion == null)
                {
                    return NotFound(new
                    {
                        error = "question_not_found",
                        message = "Không tìm thấy câu hỏi nào"
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

        [HttpPost("check")]
        public async Task<IActionResult> CheckAnswer([FromBody] QuizAnswerDto quizAnswerDto)
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
                    var resultUserId = await _quizAnswerService.CheckAnswer(quizAnswerDto.QuestionId, quizAnswerDto.SelectedAnswer);
                    return Ok(resultUserId);
                }

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

        [HttpGet("stats")]
        public async Task<IActionResult> GetQuizStatistics()
        {
            try
            {
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found, returning default stats");
                    
                    // Get total number of quiz questions
                    int totalQuestions;
                    try
                    {
                        // Use repository directly instead of calling service
                        totalQuestions = await _quizQuestionRepo.CountQuizQuestions();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error counting quiz questions");
                        totalQuestions = 0;
                    }
                    
                    return Ok(new QuizStatsDto
                    {
                        TotalQuestions = totalQuestions,
                        CompletedQuestions = 0,
                        CorrectAnswers = 0,
                        CompletionPercentage = 0,
                        CorrectPercentage = 0
                    });
                }

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