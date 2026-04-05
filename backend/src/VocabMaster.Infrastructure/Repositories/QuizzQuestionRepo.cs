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

        public async Task<QuizQuestion?> GetRandomUncompletedQuestion(int userId)
        {
            return await _context.QuizQuestions
                .Where(question => !_context.CompletedQuizzes.Any(completedQuiz =>
                    completedQuiz.UserId == userId && completedQuiz.QuizQuestionId == question.Id))
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task<QuizQuestion?> GetQuestionById(int questionId)
        {
            return await _context.QuizQuestions
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        public async Task<CompletedQuiz?> GetCompletedQuiz(int userId, int quizQuestionId)
        {
            return await _context.CompletedQuizzes
                .FirstOrDefaultAsync(completedQuiz =>
                    completedQuiz.UserId == userId && completedQuiz.QuizQuestionId == quizQuestionId);
        }

        public async Task<CompletedQuiz> SaveCompletedQuiz(int userId, int quizQuestionId, bool wasCorrect)
        {
            var existingCompletion = await GetCompletedQuiz(userId, quizQuestionId);
            if (existingCompletion != null)
            {
                return existingCompletion;
            }

            var completedQuiz = new CompletedQuiz
            {
                UserId = userId,
                QuizQuestionId = quizQuestionId,
                WasCorrect = wasCorrect,
                CompletedAt = DateTime.UtcNow
            };

            _context.CompletedQuizzes.Add(completedQuiz);

            try
            {
                await _context.SaveChangesAsync();
                return completedQuiz;
            }
            catch (DbUpdateException)
            {
                _context.Entry(completedQuiz).State = EntityState.Detached;

                var savedCompletion = await GetCompletedQuiz(userId, quizQuestionId);
                if (savedCompletion != null)
                {
                    return savedCompletion;
                }

                throw;
            }
        }
    }
}
