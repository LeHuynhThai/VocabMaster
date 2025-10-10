using AutoMapper;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Quiz;

namespace Services.Implementation
{
    public class QuizQuestionService
    {
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly ICompletedQuizRepo _completedQuizRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizQuestionService> _logger;

        public QuizQuestionService(
            IQuizQuestionRepo quizQuestionRepo,
            ICompletedQuizRepo completedQuizRepo,
            IMapper mapper,
            ILogger<QuizQuestionService> logger)
        {
            _quizQuestionRepo = quizQuestionRepo ?? throw new ArgumentNullException(nameof(quizQuestionRepo));
            _completedQuizRepo = completedQuizRepo ?? throw new ArgumentNullException(nameof(completedQuizRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<QuizQuestionDto> GetRandomQuestion()
        {
            try
            {
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                var question = await _quizQuestionRepo.GetRandomQuizQuestion();

                if (question == null)
                {
                    _logger.LogWarning("No quiz questions available");
                    return null;
                }

                _logger.LogInformation("Retrieved quiz question: Word={Word}, CorrectAnswer={CorrectAnswer}, WrongAnswer1={WrongAnswer1}, WrongAnswer2={WrongAnswer2}, WrongAnswer3={WrongAnswer3}",
                    question.Word, question.CorrectAnswer, question.WrongAnswer1, question.WrongAnswer2, question.WrongAnswer3);

                var questionDto = _mapper.Map<QuizQuestionDto>(question);

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
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                var completedQuestionIds = await _completedQuizRepo.GetCompletedQuizQuestionIdsByUserId(userId);

                var question = await _quizQuestionRepo.GetRandomUnansweredQuizQuestion(completedQuestionIds);

                if (question == null)
                {
                    int totalQuestions = await _quizQuestionRepo.CountQuizQuestions();
                    int completedCount = completedQuestionIds.Count;
                    
                    _logger.LogInformation("User {UserId} has completed all quiz questions ({CompletedCount}/{TotalQuestions})", 
                        userId, completedCount, totalQuestions);
                    
                    return null;
                }

                var questionDto = _mapper.Map<QuizQuestionDto>(question);

                return questionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random uncompleted quiz question for user {UserId}", userId);
                throw;
            }
        }
    }

    public class QuizProgressService
    {
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly ICompletedQuizRepo _completedQuizRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizProgressService> _logger;

        public QuizProgressService(
            IQuizQuestionRepo quizQuestionRepo,
            ICompletedQuizRepo completedQuizRepo,
            IMapper mapper,
            ILogger<QuizProgressService> logger)
        {
            _quizQuestionRepo = quizQuestionRepo ?? throw new ArgumentNullException(nameof(quizQuestionRepo));
            _completedQuizRepo = completedQuizRepo ?? throw new ArgumentNullException(nameof(completedQuizRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public async Task<List<CompletedQuizDto>> GetCompleteQuizz(int userId)
        {
            try
            {
                _logger.LogInformation("Getting correct quizzes for user {UserId}", userId);
                var completedQuizzes = await _completedQuizRepo.GetByUserId(userId);
                var correctQuizzes = completedQuizzes.Where(cq => cq.WasCorrect).ToList();
                return _mapper.Map<List<CompletedQuizDto>>(correctQuizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting correct quizzes for user {UserId}", userId);
                throw;
            }
        }

        public async Task<QuizStatsDto> GetQuizStatistics(int userId)
        {
            try
            {
                int totalQuestions = await _quizQuestionRepo.CountQuizQuestions();

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

        public async Task<(List<CompletedQuizDto> Items, int TotalCount, int TotalPages)> GetPaginatedCorrectQuizzes(int userId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting paginated correct quizzes for user {UserId}, page {PageNumber}, size {PageSize}", 
                    userId, pageNumber, pageSize);
                
                var (items, totalCount) = await _completedQuizRepo.GetPaginatedCorrectQuizzes(userId, pageNumber, pageSize);
                
                _logger.LogInformation("Repository returned {Count} items and {Total} total count", 
                    items?.Count ?? 0, totalCount);
                
                if (items == null)
                {
                    _logger.LogWarning("Repository returned null items for user {UserId}", userId);
                    return (new List<CompletedQuizDto>(), 0, 0);
                }
                
                var dtos = _mapper.Map<List<CompletedQuizDto>>(items);
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                
                _logger.LogInformation("Mapped {Count} DTOs for user {UserId}, total {Total} items, {Pages} pages", 
                    dtos.Count, userId, totalCount, totalPages);
                
                foreach (var dto in dtos)
                {
                    if (string.IsNullOrEmpty(dto.Word))
                    {
                        _logger.LogWarning("Quiz {Id} is missing word information", dto.Id);
                    }
                }
                
                return (dtos, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated correct quizzes for user {UserId}: {Message}", 
                    userId, ex.Message);
                throw;
            }
        }
    }

    public class QuizAnswerService
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
        public async Task<QuizResultDto> CheckAnswer(int questionId, string answer)
        {
            try
            {
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

                var result = await CheckAnswer(questionId, answer);
                _logger.LogInformation("CheckAnswer result: IsCorrect={IsCorrect}, CorrectAnswer={CorrectAnswer}",
                    result.IsCorrect, result.CorrectAnswer);
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