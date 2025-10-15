using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.DTOs;
using Repository.Entities;
using Service.Interfaces;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizzStatController : ControllerBase
    {
        private readonly IQuizzStatService _quizzStatService;

        public QuizzStatController(IQuizzStatService quizzStatService)
        {
            _quizzStatService = quizzStatService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetQuizStats()
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var totalQuestions = await _quizzStatService.GetTotalQuestions();
                var completedQuizzes = await _quizzStatService.GetCompletedQuizzes(userId);

                var completedQuestions = completedQuizzes.Count;
                var correctAnswers = completedQuizzes.Count(cq => cq.WasCorrect);

                double accuracyRate = 0;
                if (completedQuestions > 0)
                {
                    accuracyRate = (double)correctAnswers / completedQuestions * 100;
                }

                var stats = new QuizStatsDto
                {
                    TotalQuestions = totalQuestions,
                    CompletedQuestions = completedQuestions,
                    CorrectAnswers = correctAnswers,
                    AccuracyRate = accuracyRate
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("completed-answers")]
        public async Task<IActionResult> GetCompletedAnswers()
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var completedQuizzes = await _quizzStatService.GetCompletedQuizzes(userId);

                var completedAnswers = completedQuizzes.Select(cq => new CompletedQuizDto
                {
                    Id = cq.Id,
                    QuizQuestionId = cq.QuizQuestionId,
                    Word = cq.QuizQuestion.Word,
                    CorrectAnswer = cq.QuizQuestion.CorrectAnswer,
                    CompletedAt = cq.CompletedAt,
                    WasCorrect = cq.WasCorrect
                }).ToList();

                return Ok(completedAnswers);
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
