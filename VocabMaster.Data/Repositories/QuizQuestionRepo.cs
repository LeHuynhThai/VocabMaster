using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    public class QuizQuestionRepo : IQuizQuestionRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizQuestionRepo> _logger;
        private readonly Random _random = new Random();

        public QuizQuestionRepo(AppDbContext context, ILogger<QuizQuestionRepo> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
        
        public async Task<QuizQuestion> GetRandomUnansweredQuizQuestion(List<int> completedQuestionIds)
        {
            try
            {
                // take all the quiz uncomplete
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