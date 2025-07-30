using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    /// <summary>
    /// Repository for quiz question operations
    /// </summary>
    public class QuizQuestionRepo : IQuizQuestionRepo
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        public QuizQuestionRepo(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a random quiz question
        /// </summary>
        /// <returns>A random quiz question</returns>
        public async Task<QuizQuestion> GetRandomQuizQuestion()
        {
            var count = await _context.QuizQuestions.CountAsync();
            
            if (count == 0)
                return null;
                
            // Get a random quiz question
            var skip = _random.Next(0, count);
            return await _context.QuizQuestions.Skip(skip).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a random quiz question excluding specific IDs
        /// </summary>
        /// <param name="excludeIds">IDs to exclude</param>
        /// <returns>A random quiz question not in the excluded IDs</returns>
        public async Task<QuizQuestion> GetRandomUnansweredQuizQuestion(List<int> excludeIds)
        {
            // If no IDs to exclude, just get a random question
            if (excludeIds == null || !excludeIds.Any())
                return await GetRandomQuizQuestion();

            // Get all quiz questions that are not in the excluded IDs
            var availableQuestions = await _context.QuizQuestions
                .Where(q => !excludeIds.Contains(q.Id))
                .ToListAsync();

            if (!availableQuestions.Any())
                return null;

            // Select a random question
            var randomIndex = _random.Next(0, availableQuestions.Count);
            return availableQuestions[randomIndex];
        }
        
        /// <summary>
        /// Gets a quiz question by id
        /// </summary>
        /// <param name="id">The quiz question id</param>
        /// <returns>The quiz question if found, null otherwise</returns>
        public async Task<QuizQuestion> GetQuizQuestionById(int id)
        {
            return await _context.QuizQuestions.FindAsync(id);
        }
        
        /// <summary>
        /// Creates a new quiz question
        /// </summary>
        /// <param name="quizQuestion">The quiz question to create</param>
        /// <returns>The created quiz question</returns>
        public async Task<QuizQuestion> CreateQuizQuestion(QuizQuestion quizQuestion)
        {
            await _context.QuizQuestions.AddAsync(quizQuestion);
            await _context.SaveChangesAsync();
            return quizQuestion;
        }
        
        /// <summary>
        /// Checks if there are any quiz questions
        /// </summary>
        /// <returns>True if there are quiz questions, false otherwise</returns>
        public async Task<bool> AnyQuizQuestions()
        {
            return await _context.QuizQuestions.AnyAsync();
        }

        /// <summary>
        /// Gets the total number of quiz questions
        /// </summary>
        /// <returns>The total number of quiz questions</returns>
        public async Task<int> CountQuizQuestions()
        {
            return await _context.QuizQuestions.CountAsync();
        }
    }
} 