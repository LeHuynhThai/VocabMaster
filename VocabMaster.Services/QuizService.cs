using AutoMapper;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly IVocabularyRepo _vocabularyRepo;
        private readonly ICompletedQuizRepo _completedQuizRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizService> _logger;
        private readonly Random _random = new Random();

        public QuizService(
            IQuizQuestionRepo quizQuestionRepo,
            IVocabularyRepo vocabularyRepo,
            ICompletedQuizRepo completedQuizRepo,
            IMapper mapper,
            ILogger<QuizService> logger)
        {
            _quizQuestionRepo = quizQuestionRepo;
            _vocabularyRepo = vocabularyRepo;
            _completedQuizRepo = completedQuizRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuizQuestionDto> GetRandomQuestion()
        {
            try
            {
                // Check if we have any quiz questions
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                // Get a random quiz question
                var question = await _quizQuestionRepo.GetRandomQuizQuestion();

                if (question == null)
                {
                    _logger.LogWarning("No quiz questions available");
                    return null;
                }

                // Log the question details for debugging
                _logger.LogInformation("Retrieved quiz question: Word={Word}, CorrectAnswer={CorrectAnswer}, WrongAnswer1={WrongAnswer1}, WrongAnswer2={WrongAnswer2}, WrongAnswer3={WrongAnswer3}",
                    question.Word, question.CorrectAnswer, question.WrongAnswer1, question.WrongAnswer2, question.WrongAnswer3);

                // Map to DTO
                var questionDto = _mapper.Map<QuizQuestionDto>(question);

                // Log the DTO details to verify mapping
                _logger.LogInformation("Mapped to DTO: Word={Word}, CorrectAnswer={CorrectAnswer}, WrongAnswer1={WrongAnswer1}, WrongAnswer2={WrongAnswer2}, WrongAnswer3={WrongAnswer3}",
                    questionDto.Word, questionDto.CorrectAnswer, questionDto.WrongAnswer1, questionDto.WrongAnswer2, questionDto.WrongAnswer3);

                return questionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random quiz question");
                throw;
            }
        }

        public async Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId)
        {
            try
            {
                // Check if we have any quiz questions
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                // Get IDs of completed questions for this user
                var completedQuestionIds = await _completedQuizRepo.GetCompletedQuizQuestionIdsByUserId(userId);

                // Get a random quiz question that hasn't been completed by this user
                var question = await _quizQuestionRepo.GetRandomUnansweredQuizQuestion(completedQuestionIds);

                // If all questions have been answered, get a random question
                if (question == null)
                {
                    _logger.LogInformation("User {UserId} has completed all quiz questions, returning a random one", userId);
                    question = await _quizQuestionRepo.GetRandomQuizQuestion();
                }

                if (question == null)
                {
                    _logger.LogWarning("No quiz questions available for user {UserId}", userId);
                    return null;
                }

                // Map to DTO
                var questionDto = _mapper.Map<QuizQuestionDto>(question);

                return questionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random uncompleted quiz question for user {UserId}", userId);
                throw;
            }
        }

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

                // If answer is correct, mark it as completed
                if (result.IsCorrect)
                {
                    _logger.LogInformation("Answer is correct, creating CompletedQuiz record");
                    var completedQuiz = new CompletedQuiz
                    {
                        UserId = userId,
                        QuizQuestionId = questionId,
                        WasCorrect = true
                    };

                    _logger.LogInformation("Calling MarkAsCompleted with UserId={UserId}, QuizQuestionId={QuizQuestionId}, WasCorrect=true",
                        completedQuiz.UserId, completedQuiz.QuizQuestionId);

                    try
                    {
                        var markedResult = await _completedQuizRepo.MarkAsCompleted(completedQuiz);
                        _logger.LogInformation("MarkAsCompleted successful. Result Id={ResultId}", markedResult.Id);
                    }
                    catch (Exception markEx)
                    {
                        _logger.LogError(markEx, "Error in MarkAsCompleted for correct answer");
                        throw;
                    }
                }
                else
                {
                    // Even if answer is wrong, we mark it as attempted but with wasCorrect = false
                    _logger.LogInformation("Answer is incorrect, creating CompletedQuiz record with WasCorrect=false");
                    var completedQuiz = new CompletedQuiz
                    {
                        UserId = userId,
                        QuizQuestionId = questionId,
                        WasCorrect = false
                    };

                    _logger.LogInformation("Calling MarkAsCompleted with UserId={UserId}, QuizQuestionId={QuizQuestionId}, WasCorrect=false",
                        completedQuiz.UserId, completedQuiz.QuizQuestionId);

                    try
                    {
                        var markedResult = await _completedQuizRepo.MarkAsCompleted(completedQuiz);
                        _logger.LogInformation("MarkAsCompleted successful. Result Id={ResultId}", markedResult.Id);
                    }
                    catch (Exception markEx)
                    {
                        _logger.LogError(markEx, "Error in MarkAsCompleted for incorrect answer");
                        throw;
                    }
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

        public async Task<List<CompletedQuizDto>> GetCompletedQuizzes(int userId)
        {
            try
            {
                var completedQuizzes = await _completedQuizRepo.GetByUserId(userId);
                return _mapper.Map<List<CompletedQuizDto>>(completedQuizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed quizzes for user {UserId}", userId);
                throw;
            }
        }

        public async Task<QuizStatsDto> GetQuizStatistics(int userId)
        {
            try
            {
                // Get total number of quiz questions
                int totalQuestions = await _quizQuestionRepo.CountQuizQuestions();

                // Get user's completed quizzes
                var completedQuizzes = await _completedQuizRepo.GetByUserId(userId);
                int completedCount = completedQuizzes.Count;
                int correctCount = completedQuizzes.Count(cq => cq.WasCorrect);

                return new QuizStatsDto
                {
                    TotalQuestions = totalQuestions,
                    CompletedQuestions = completedCount,
                    CorrectAnswers = correctCount,
                    CompletionPercentage = totalQuestions > 0 ? (double)completedCount / totalQuestions * 100 : 0,
                    CorrectPercentage = completedCount > 0 ? (double)correctCount / completedCount * 100 : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz statistics for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> CountTotalQuestions()
        {
            try
            {
                return await _quizQuestionRepo.CountQuizQuestions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting total quiz questions");
                return 0;
            }
        }
    }
}