using AutoMapper;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Quiz;

namespace VocabMaster.Services.Quiz
{
    public class QuizProgressService : IQuizProgressService
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

        // Get only correct quizzes for a user
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

        // Get quiz statistics for a user
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

        // Get paginated correct quizzes for a user
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
                
                // Ensure each DTO has the word from the quiz question
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
}