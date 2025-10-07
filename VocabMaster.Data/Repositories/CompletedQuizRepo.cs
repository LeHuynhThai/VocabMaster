using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    public class CompletedQuizRepo : ICompletedQuizRepo
    {
        private readonly AppDbContext _context;

        public CompletedQuizRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompletedQuiz>> GetByUserId(int userId)
        {
            return await _context.CompletedQuizzes
                .Where(cq => cq.UserId == userId)
                .Include(cq => cq.QuizQuestion)
                .ToListAsync();
        }

        public async Task<List<int>> GetCompletedQuizQuestionIdsByUserId(int userId)
        {
            return await _context.CompletedQuizzes
                .Where(cq => cq.UserId == userId)
                .Select(cq => cq.QuizQuestionId)
                .ToListAsync();
        }

        public async Task<bool> IsQuizQuestionCompletedByUser(int userId, int quizQuestionId)
        {
            return await _context.CompletedQuizzes
                .AnyAsync(cq => cq.UserId == userId && cq.QuizQuestionId == quizQuestionId);
        }

        public async Task<CompletedQuiz> MarkAsCompleted(CompletedQuiz completedQuiz)
        {

                var existingRecord = await _context.CompletedQuizzes
                    .FirstOrDefaultAsync(cq => cq.UserId == completedQuiz.UserId && cq.QuizQuestionId == completedQuiz.QuizQuestionId);

                    await _context.CompletedQuizzes.AddAsync(completedQuiz);
                    await _context.SaveChangesAsync();
                
            return completedQuiz;
        }
        
        public async Task<(List<CompletedQuiz> Items, int TotalCount)> GetPaginatedCorrectQuizzes(int userId, int pageNumber, int pageSize)
        {        
                var query = _context.CompletedQuizzes
                    .Where(cq => cq.UserId == userId && cq.WasCorrect)
                    .Include(cq => cq.QuizQuestion)
                    .OrderByDescending(cq => cq.CompletedAt);
                
                int totalCount = await query.CountAsync();
                
                if (totalCount == 0)
                {
                    return (new List<CompletedQuiz>(), 0);
                }
                
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                
                foreach (var item in items)
                {
                    if (item.QuizQuestion == null)
                    {

                    item.QuizQuestion = await _context.QuizQuestions.FindAsync(item.QuizQuestionId);
                    
                }
            }
                
            return (items, totalCount);
        }
    }
}