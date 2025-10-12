using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Implementation
{
    public class VocabRepo : IVocabularyRepo
    {
        private readonly AppDbContext _context;

        public VocabRepo(AppDbContext context)
        {
            _context = context;
        }

        // Get random word using for get random word exclude learned words
        public async Task<Vocabulary?> GetRandom()
        {
            var count = await _context.Vocabularies.CountAsync();
            var skipCount = new Random().Next(count);
            return await _context.Vocabularies
                .Skip(skipCount)                    
                .Take(1)
                .FirstOrDefaultAsync();
        }

        // Get random word exclude learned words
        public async Task<Vocabulary?> GetRandomExcludeLearned(List<string> learnedWords)
        {
            var count = await _context.Vocabularies
                .Where(v => !learnedWords.Contains(v.Word))
                .CountAsync();
            var skipCount = new Random().Next(count);
            return await _context.Vocabularies
                .Where(v => !learnedWords.Contains(v.Word))
                .Skip(skipCount)
                .Take(1)
                .FirstOrDefaultAsync();
        }

        // Update a vocabulary, use for crawl Vietnamese from api
        public async Task<bool> Update(Vocabulary vocabulary)
        {
            var existingVocabulary = await _context.Vocabularies.FindAsync(vocabulary.Id);
            existingVocabulary.Vietnamese = vocabulary.Vietnamese;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


