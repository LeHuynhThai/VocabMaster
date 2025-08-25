using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    // Repository thao tác với dữ liệu câu hỏi trắc nghiệm
    public class QuizQuestionRepo : IQuizQuestionRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizQuestionRepo> _logger;
        private readonly Random _random = new Random();

        // Hàm khởi tạo repository, inject context và logger
        public QuizQuestionRepo(AppDbContext context, ILogger<QuizQuestionRepo> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Lấy một câu hỏi trắc nghiệm ngẫu nhiên
        public async Task<QuizQuestion> GetRandomQuizQuestion()
        {
            try
            {
                int count = await _context.QuizQuestions.CountAsync();

                if (count == 0)
                    return null;

                int randomIndex = new Random().Next(0, count);

                return await _context.QuizQuestions
                    .OrderBy(q => q.Id)
                    .Skip(randomIndex)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRandomQuizQuestion: {Message}", ex.Message);
                throw;
            }
        }

        // Lấy một câu hỏi trắc nghiệm mà user chưa trả lời
        public async Task<QuizQuestion> GetRandomUnansweredQuizQuestion(List<int> completedQuestionIds)
        {
            try
            {
                // Lấy tất cả các câu hỏi chưa hoàn thành
                var unansweredQuestions = await _context.QuizQuestions
                    .Where(q => !completedQuestionIds.Contains(q.Id))
                    .ToListAsync();

                if (!unansweredQuestions.Any())
                    return null;

                int randomIndex = new Random().Next(0, unansweredQuestions.Count);
                return unansweredQuestions[randomIndex];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRandomUnansweredQuizQuestion");
                throw;
            }
        }

        // Lấy câu hỏi trắc nghiệm theo Id
        public async Task<QuizQuestion> GetQuizQuestionById(int id)
        {
            return await _context.QuizQuestions.FindAsync(id);
        }

        // Thêm mới một câu hỏi trắc nghiệm
        public async Task<QuizQuestion> CreateQuizQuestion(QuizQuestion quizQuestion)
        {
            await _context.QuizQuestions.AddAsync(quizQuestion);
            await _context.SaveChangesAsync();
            return quizQuestion;
        }

        // Kiểm tra có tồn tại câu hỏi trắc nghiệm nào không
        public async Task<bool> AnyQuizQuestions()
        {
            return await _context.QuizQuestions.AnyAsync();
        }

        // Đếm tổng số câu hỏi trắc nghiệm
        public async Task<int> CountQuizQuestions()
        {
            return await _context.QuizQuestions.CountAsync();
        }
    }
}