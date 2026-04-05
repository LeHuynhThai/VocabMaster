using Microsoft.EntityFrameworkCore;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;
using VocabMaster.Infrastructure.Persistence;

namespace VocabMaster.Infrastructure.Repositories
{
    public class QuizzStatRepo : IQuizzStatRepo
    {
        private readonly ApplicationDbContext _context;

        public QuizzStatRepo(ApplicationDbContext context)
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

    }
}
