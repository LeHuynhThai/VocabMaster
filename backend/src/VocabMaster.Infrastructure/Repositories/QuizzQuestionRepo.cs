using Microsoft.EntityFrameworkCore;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;
using VocabMaster.Infrastructure.Persistence;

namespace VocabMaster.Infrastructure.Repositories
{
    public class QuizzQuestionRepo : IQuizzQuestionRepo
    {
        private readonly ApplicationDbContext _context;

        public QuizzQuestionRepo(ApplicationDbContext context)
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

        public async Task<QuizQuestion?> GetQuestionById(int questionId)
        {
            return await _context.QuizQuestions
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        public async Task<bool> SaveCompletedQuiz(int userId, int quizQuestionId, bool wasCorrect)
        {
            var completedQuiz = new CompletedQuiz
            {
                UserId = userId,
                QuizQuestionId = quizQuestionId,
                WasCorrect = wasCorrect,
                CompletedAt = DateTime.UtcNow
            };

            _context.CompletedQuizzes.Add(completedQuiz);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
