using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Implementation
{
    public class QuizQuestionRepo : IQuizQuestionRepo
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        public QuizQuestionRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuizQuestion> GetRandomQuizQuestion()
        {
                int count = await _context.QuizQuestions.CountAsync();

                int randomIndex = new Random().Next(0, count);

                return await _context.QuizQuestions
                    .OrderBy(q => q.Id)
                    .Skip(randomIndex)
                    .FirstOrDefaultAsync();
        }

        public async Task<QuizQuestion> GetRandomUnansweredQuizQuestion(List<int> completedQuestionIds)
        {
                var unansweredQuestions = await _context.QuizQuestions
                .Where(q => !completedQuestionIds.Contains(q.Id))
                .ToListAsync();

            if (!unansweredQuestions.Any())
                return null;

                int randomIndex = new Random().Next(0, unansweredQuestions.Count);
            return unansweredQuestions[randomIndex];
        }

        public async Task<QuizQuestion> GetQuizQuestionById(int id)
        {
            return await _context.QuizQuestions.FindAsync(id);
        }
        public async Task<QuizQuestion> CreateQuizQuestion(QuizQuestion quizQuestion)
        {
            await _context.QuizQuestions.AddAsync(quizQuestion);
            await _context.SaveChangesAsync();
            return quizQuestion;
        }

        public async Task<bool> AnyQuizQuestions()
        {
            return await _context.QuizQuestions.AnyAsync();
        }
        public async Task<int> CountQuizQuestions()
        {
            return await _context.QuizQuestions.CountAsync();
        }
    }
}