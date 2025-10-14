using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Implementation
{
    public class QuizzQuestionRepo : IQuizzQuestionRepo
    {
        private readonly AppDbContext _context;

        public QuizzQuestionRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetCompletedQuestionIds(int userId)
        {
            return await _context.CompletedQuizzes
                .Where(cq => cq.UserId == userId)
                .Select(cq => cq.QuizQuestionId)
                .ToListAsync();
        }

        public async Task<QuizQuestion?> GetRandomUncompletedQuestion(int userId)
        {
            var completedQuestionIds = await GetCompletedQuestionIds(userId);
            
            var randomQuestion = await _context.QuizQuestions
                .Where(q => !completedQuestionIds.Contains(q.Id))
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();

            return randomQuestion;
        }
    }
}
