using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.DTOs;
using Service.Interfaces;
using System.Security.Claims;

namespace API.Controllers
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
                var question = await _quizzQuestionService.GetRandomUncompletedQuestion(userId);
                
                if (question == null)
                {
                    return Ok(new { 
                        message = "Bạn đã hoàn thành tất cả câu hỏi!",
                        completed = true 
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> SubmitQuizAnswer([FromBody] SubmitAnswerRequest request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
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
            catch (Exception ex)
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
    }
}
