using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Implementation
{
    public class QuizzStatRepo : IQuizzStatRepo
    {
        private readonly AppDbContext _context;

        public QuizzStatRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalQuestions()
        {
            return await _context.QuizQuestions.CountAsync();
        }

        public async Task<List<CompletedQuiz>> GetCompletedQuizzes(int userId)
        {
            return await _context.CompletedQuizzes
                .Where(cq => cq.UserId == userId)
                .Include(cq => cq.QuizQuestion)
                .OrderByDescending(cq => cq.CompletedAt)
                .ToListAsync();
        }

        public async Task<List<CompletedQuiz>> GetCorrectAnswers(int userId)
        {
            return await _context.CompletedQuizzes
                .Where(cq => cq.UserId == userId && cq.WasCorrect)
                .Include(cq => cq.QuizQuestion)
                .OrderByDescending(cq => cq.CompletedAt)
                .ToListAsync();
        }
    }
}
