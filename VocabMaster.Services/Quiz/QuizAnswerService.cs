using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Quiz;

// This service is used to check the answer of a quiz question and mark the question as completed if the answer is correct
namespace VocabMaster.Services.Quiz
{
    public class QuizAnswerService : IQuizAnswerService
    {
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly ICompletedQuizRepo _completedQuizRepo;
        private readonly ILogger<QuizAnswerService> _logger;

        public QuizAnswerService(
            IQuizQuestionRepo quizQuestionRepo,
            ICompletedQuizRepo completedQuizRepo,
            ILogger<QuizAnswerService> logger)
        {
            _quizQuestionRepo = quizQuestionRepo ?? throw new ArgumentNullException(nameof(quizQuestionRepo));
            _completedQuizRepo = completedQuizRepo ?? throw new ArgumentNullException(nameof(completedQuizRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        // Check the answer of a quiz question
        public async Task<QuizResultDto> CheckAnswer(int questionId, string answer)
        {
            try
            {
                // Get the quiz question
                var question = await _quizQuestionRepo.GetQuizQuestionById(questionId);

                if (question == null)
                {
                    _logger.LogWarning("Quiz question not found: {QuestionId}", questionId);
                    return new QuizResultDto
                    {
                        IsCorrect = false,
                        CorrectAnswer = null,
                        Message = "Câu hỏi không tồn tại"
                    };
                }

                // Check if the answer is correct
                bool isCorrect = question.CorrectAnswer == answer;

                return new QuizResultDto
                {
                    IsCorrect = isCorrect,
                    CorrectAnswer = question.CorrectAnswer,
                    Message = isCorrect ? "Chính xác!" : "Không chính xác. Hãy thử lại!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiz answer");
                throw;
            }
        }
        
        // Check the answer of a quiz question and mark the question as completed if the answer is correct
        public async Task<QuizResultDto> CheckAnswerAndMarkCompleted(int questionId, string answer, int userId)
        {
            try
            {
                _logger.LogInformation("CheckAnswerAndMarkCompleted called with QuestionId={QuestionId}, Answer={Answer}, UserId={UserId}",
                    questionId, answer, userId);

                // Get the result first
                var result = await CheckAnswer(questionId, answer);
                _logger.LogInformation("CheckAnswer result: IsCorrect={IsCorrect}, CorrectAnswer={CorrectAnswer}",
                    result.IsCorrect, result.CorrectAnswer);

                // Create CompletedQuiz record with appropriate WasCorrect value
                var completedQuiz = new CompletedQuiz
                {
                    UserId = userId,
                    QuizQuestionId = questionId,
                    WasCorrect = result.IsCorrect
                };

                _logger.LogInformation("Calling MarkAsCompleted with UserId={UserId}, QuizQuestionId={QuizQuestionId}, WasCorrect={WasCorrect}",
                    completedQuiz.UserId, completedQuiz.QuizQuestionId, completedQuiz.WasCorrect);

                try
                {
                    var markedResult = await _completedQuizRepo.MarkAsCompleted(completedQuiz);
                    _logger.LogInformation("MarkAsCompleted successful. Result Id={ResultId}", markedResult.Id);
                }
                catch (Exception markEx)
                {
                    _logger.LogError(markEx, "Error in MarkAsCompleted");
                    throw;
                }

                _logger.LogInformation("CheckAnswerAndMarkCompleted completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and marking quiz answer for user {UserId}", userId);
                throw;
            }
        }
    }
}