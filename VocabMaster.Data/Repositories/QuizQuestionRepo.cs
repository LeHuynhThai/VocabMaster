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
    }
} 