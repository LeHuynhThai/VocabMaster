using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
} 