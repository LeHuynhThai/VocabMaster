using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(
            IQuizService quizService,
            ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a random quiz question
        /// </summary>
        /// <returns>Random quiz question</returns>
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomQuestion()
        {
            try
            {
                var question = await _quizService.GetRandomQuestion();

                if (question == null)
                {
                    return NotFound(new { message = "Không tìm thấy câu hỏi nào" });
                }

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random quiz question");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải câu hỏi" });
            }
        }

        /// <summary>
        /// Gets a random uncompleted quiz question for the current user
        /// </summary>
        /// <returns>Random uncompleted quiz question</returns>
        [HttpGet("random-uncompleted")]
        public async Task<IActionResult> GetRandomUncompletedQuestion()
        {
            try
            {
                // Xử lý trường hợp không có userId
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    // Nếu không lấy được ID người dùng, trả về câu hỏi ngẫu nhiên thông thường
                    _logger.LogWarning("User ID not found, returning random question instead");
                    var question = await _quizService.GetRandomQuestion();

                    if (question == null)
                    {
                        return NotFound(new { message = "Không tìm thấy câu hỏi nào" });
                    }

                    return Ok(question);
                }

                // Trường hợp có userId
                var uncompletedQuestion = await _quizService.GetRandomUncompletedQuestion(userId.Value);

                if (uncompletedQuestion == null)
                {
                    return NotFound(new { message = "Không tìm thấy câu hỏi nào" });
                }

                return Ok(uncompletedQuestion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random uncompleted quiz question");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải câu hỏi" });
            }
        }

        /// <summary>
        /// Creates a new quiz question from vocabulary
        /// </summary>
        /// <returns>Created quiz question</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion()
        {
            try
            {
                var question = await _quizService.CreateQuizQuestionFromVocabulary();

                if (question == null)
                {
                    return BadRequest(new { message = "Không thể tạo câu hỏi mới" });
                }

                return Ok(new { message = "Đã tạo câu hỏi mới", questionId = question.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz question");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo câu hỏi" });
            }
        }

        /// <summary>
        /// Checks the answer to a quiz question
        /// </summary>
        /// <param name="quizAnswerDto">The answer to check</param>
        /// <returns>Result of the answer check</returns>
        [HttpPost("check-answer")]
        public async Task<IActionResult> CheckAnswer([FromBody] QuizAnswerDto quizAnswerDto)
        {
            try
            {
                if (quizAnswerDto == null)
                {
                    return BadRequest(new { message = "Dữ liệu câu trả lời không hợp lệ" });
                }

                var result = await _quizService.CheckAnswer(quizAnswerDto.QuestionId, quizAnswerDto.SelectedAnswer);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiz answer");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra câu trả lời" });
            }
        }

        /// <summary>
        /// Checks the answer to a quiz question and marks it as completed if correct
        /// </summary>
        /// <param name="quizAnswerDto">The answer to check</param>
        /// <returns>Result of the answer check</returns>
        [HttpPost("check-answer-and-complete")]
        public async Task<IActionResult> CheckAnswerAndMarkCompleted([FromBody] QuizAnswerDto quizAnswerDto)
        {
            try
            {
                if (quizAnswerDto == null)
                {
                    return BadRequest(new { message = "Dữ liệu câu trả lời không hợp lệ" });
                }

                // Xử lý trường hợp không có userId
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    // Nếu không lấy được ID người dùng, chỉ kiểm tra câu trả lời mà không đánh dấu
                    _logger.LogWarning("User ID not found, checking answer without marking completed");
                    var resultUserId = await _quizService.CheckAnswer(quizAnswerDto.QuestionId, quizAnswerDto.SelectedAnswer);
                    return Ok(resultUserId);
                }

                var result = await _quizService.CheckAnswerAndMarkCompleted(
                    quizAnswerDto.QuestionId,
                    quizAnswerDto.SelectedAnswer,
                    userId.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and marking quiz answer");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi kiểm tra câu trả lời" });
            }
        }

        /// <summary>
        /// Gets all completed quiz questions for the current user
        /// </summary>
        /// <returns>List of completed quiz questions</returns>
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedQuizzes()
        {
            try
            {
                // Xử lý trường hợp không có userId
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    // Nếu không lấy được ID người dùng, trả về danh sách rỗng
                    _logger.LogWarning("User ID not found, returning empty completed list");
                    return Ok(new List<CompletedQuizDto>());
                }

                var completedQuizzes = await _quizService.GetCompletedQuizzes(userId.Value);
                return Ok(completedQuizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed quizzes");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải danh sách câu hỏi đã hoàn thành" });
            }
        }

        /// <summary>
        /// Gets statistics about the current user's quiz progress
        /// </summary>
        /// <returns>Quiz statistics</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetQuizStatistics()
        {
            try
            {
                // Xử lý trường hợp không có userId
                var userId = GetUserIdSafe();
                if (userId == null)
                {
                    // Nếu không lấy được ID người dùng, trả về thống kê mặc định
                    _logger.LogWarning("User ID not found, returning default stats");
                    return Ok(new QuizStatsDto
                    {
                        TotalQuestions = await _quizService.CountTotalQuestions(),
                        CompletedQuestions = 0,
                        CorrectAnswers = 0,
                        CompletionPercentage = 0,
                        CorrectPercentage = 0
                    });
                }

                var stats = await _quizService.GetQuizStatistics(userId.Value);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz statistics");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải thống kê trắc nghiệm" });
            }
        }

        /// <summary>
        /// Gets the current user ID from claims safely
        /// </summary>
        /// <returns>The current user ID or null if not found</returns>
        private int? GetUserIdSafe()
        {
            try
            {
                // Kiểm tra nhiều loại claim có thể chứa user id
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                              ?? User.Claims.FirstOrDefault(c => c.Type == "userId")
                              ?? User.Claims.FirstOrDefault(c => c.Type == "id")
                              ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                // Ghi log chi tiết claims hiện có để gỡ lỗi
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
        /// Gets the current user ID from claims
        /// </summary>
        /// <returns>The current user ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user ID is not found in claims</exception>
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