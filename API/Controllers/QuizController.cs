using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomQuestion()
        {
            try
            {

                var question = await _quizService.GetRandomQuestion();

                if (question == null)
                {
                    return NotFound(new
                    {
                        error = "question_not_found",
                        message = "Không tìm thấy câu hỏi nào. Vui lòng thêm câu hỏi vào hệ thống."
                    });
                }

                return Ok(question);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "server_error",
                    message = "Đã xảy ra lỗi khi tải câu hỏi. Vui lòng thử lại sau.",
                    details = ex.Message
                });
            }
        }
    }
}