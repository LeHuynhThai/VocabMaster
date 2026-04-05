using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Api.Contracts.Common;
using VocabMaster.Api.Contracts.Quiz;
using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;

namespace VocabMaster.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizzController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IQuizzQuestionService _quizzQuestionService;

        public QuizzController(ICurrentUserService currentUserService, IQuizzQuestionService quizzQuestionService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _quizzQuestionService = quizzQuestionService;
        }

        [HttpGet("random-question")]
        public async Task<IActionResult> GetRandomUncompletedQuestion()
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

                var question = await _quizzQuestionService.GetRandomUncompletedQuestion(userId.Value);

                if (question == null)
                {
                    return Ok(new QuizCompletionStatusResponse
                    {
                        Message = "Bạn đã hoàn thành tất cả câu hỏi!",
                        Completed = true,
                        AllCompleted = true
                    });
                }

                return Ok(ToQuizQuestionResponse(question));
            }
            catch (Exception)
            {
                return StatusCode(500, new MessageResponse { Message = "Internal server error" });
            }
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> SubmitQuizAnswer([FromBody] SubmitQuizAnswerRequest request)
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

                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.SelectedAnswer))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "validation_error",
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var isCorrect = await _quizzQuestionService.SubmitQuizAnswer(
                    userId.Value,
                    request.QuizQuestionId,
                    request.SelectedAnswer
                );

                return Ok(new SubmitQuizAnswerResponse
                {
                    IsCorrect = isCorrect,
                    Message = isCorrect ? "Chúc mừng! Bạn đã trả lời đúng." : "Rất tiếc! Đáp án không đúng."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new MessageResponse { Message = "Internal server error" });
            }
        }

        private static QuizQuestionResponse ToQuizQuestionResponse(QuizQuestion question)
        {
            return new QuizQuestionResponse
            {
                Id = question.Id,
                Word = question.Word,
                CorrectAnswer = question.CorrectAnswer,
                WrongAnswer1 = question.WrongAnswer1,
                WrongAnswer2 = question.WrongAnswer2,
                WrongAnswer3 = question.WrongAnswer3
            };
        }
    }
}
