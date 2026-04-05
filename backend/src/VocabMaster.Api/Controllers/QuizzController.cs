using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VocabMaster.Application.Interfaces;

namespace VocabMaster.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizzController : ControllerBase
    {
        private readonly IQuizzQuestionService _quizzQuestionService;

        public QuizzController(IQuizzQuestionService quizzQuestionService)
        {
            _quizzQuestionService = quizzQuestionService;
        }

        [HttpGet("random-question")]
        public async Task<IActionResult> GetRandomUncompletedQuestion()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var question = await _quizzQuestionService.GetRandomUncompletedQuestion(userId);

                if (question == null)
                {
                    return Ok(new
                    {
                        message = "Bạn đã hoàn thành tất cả câu hỏi!",
                        completed = true,
                        allCompleted = true
                    });
                }

                return Ok(new
                {
                    id = question.Id,
                    word = question.Word,
                    correctAnswer = question.CorrectAnswer,
                    wrongAnswer1 = question.WrongAnswer1,
                    wrongAnswer2 = question.WrongAnswer2,
                    wrongAnswer3 = question.WrongAnswer3
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> SubmitQuizAnswer([FromBody] SubmitQuizAnswerRequest request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                if (request == null || request.QuizQuestionId <= 0 || string.IsNullOrWhiteSpace(request.SelectedAnswer))
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });
                }

                var isCorrect = await _quizzQuestionService.SubmitQuizAnswer(
                    userId,
                    request.QuizQuestionId,
                    request.SelectedAnswer
                );

                return Ok(new
                {
                    isCorrect = isCorrect,
                    message = isCorrect ? "Chúc mừng! Bạn đã trả lời đúng." : "Rất tiếc! Đáp án không đúng."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
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

        public sealed class SubmitQuizAnswerRequest
        {
            public int QuizQuestionId { get; set; }
            public string SelectedAnswer { get; set; } = string.Empty;
        }
    }
}
