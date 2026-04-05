using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Api.Contracts.Common;
using VocabMaster.Api.Contracts.Quiz;
using VocabMaster.Application.Interfaces;

namespace VocabMaster.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizzStatController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IQuizzStatService _quizzStatService;

        public QuizzStatController(ICurrentUserService currentUserService, IQuizzStatService quizzStatService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _quizzStatService = quizzStatService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetQuizStats()
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

                var stats = await _quizzStatService.GetQuizStats(userId.Value);

                return Ok(new QuizStatsResponse
                {
                    TotalQuestions = stats.TotalQuestions,
                    CompletedQuestions = stats.CompletedQuestions,
                    CorrectAnswers = stats.CorrectAnswers,
                    AccuracyRate = stats.AccuracyRate
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new MessageResponse { Message = "Internal server error" });
            }
        }

        [HttpGet("completed-answers")]
        public async Task<IActionResult> GetCompletedAnswers()
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

                var completedAnswers = await _quizzStatService.GetCompletedAnswers(userId.Value);

                return Ok(completedAnswers.Select(answer => new CompletedQuizAnswerResponse
                {
                    Id = answer.Id,
                    QuizQuestionId = answer.QuizQuestionId,
                    Word = answer.Word,
                    CorrectAnswer = answer.CorrectAnswer,
                    CompletedAt = answer.CompletedAt,
                    WasCorrect = answer.WasCorrect
                }));
            }
            catch (Exception)
            {
                return StatusCode(500, new MessageResponse { Message = "Internal server error" });
            }
        }
    }
}
