using AutoMapper;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Quiz;

namespace VocabMaster.Services.Quiz
{
    // Service xử lý logic lấy câu hỏi trắc nghiệm (random, chưa làm, ...)
    public class QuizQuestionService : IQuizQuestionService
    {
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly ICompletedQuizRepo _completedQuizRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizQuestionService> _logger;

        // Hàm khởi tạo service, inject các dependency cần thiết
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

        // Lấy một câu hỏi trắc nghiệm ngẫu nhiên
        public async Task<QuizQuestionDto> GetRandomQuestion()
        {
            try
            {
                // Kiểm tra có câu hỏi nào không
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                // Lấy một câu hỏi ngẫu nhiên
                var question = await _quizQuestionRepo.GetRandomQuizQuestion();

                if (question == null)
                {
                    _logger.LogWarning("No quiz questions available");
                    return null;
                }

                // Log chi tiết câu hỏi để debug
                _logger.LogInformation("Retrieved quiz question: Word={Word}, CorrectAnswer={CorrectAnswer}, WrongAnswer1={WrongAnswer1}, WrongAnswer2={WrongAnswer2}, WrongAnswer3={WrongAnswer3}",
                    question.Word, question.CorrectAnswer, question.WrongAnswer1, question.WrongAnswer2, question.WrongAnswer3);

                // Map sang DTO
                var questionDto = _mapper.Map<QuizQuestionDto>(question);

                // Log chi tiết DTO để kiểm tra mapping
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

        // Lấy một câu hỏi trắc nghiệm mà user chưa làm
        public async Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId)
        {
            try
            {
                // Kiểm tra có câu hỏi nào không
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                // Lấy danh sách id các câu hỏi đã hoàn thành
                var completedQuestionIds = await _completedQuizRepo.GetCompletedQuizQuestionIdsByUserId(userId);

                // Lấy một câu hỏi ngẫu nhiên mà user chưa làm
                var question = await _quizQuestionRepo.GetRandomUnansweredQuizQuestion(completedQuestionIds);

                // Nếu user đã làm hết thì trả về null
                if (question == null)
                {
                    // Đếm tổng số câu hỏi
                    int totalQuestions = await _quizQuestionRepo.CountQuizQuestions();
                    int completedCount = completedQuestionIds.Count;
                    
                    _logger.LogInformation("User {UserId} has completed all quiz questions ({CompletedCount}/{TotalQuestions})", 
                        userId, completedCount, totalQuestions);
                    
                    // Đã làm hết
                    return null;
                }

                // Map sang DTO
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
}