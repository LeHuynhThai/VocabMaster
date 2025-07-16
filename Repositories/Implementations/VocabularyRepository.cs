using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Entities;
using VocabMaster.Repositories.Interfaces;

namespace VocabMaster.Repositories.Implementations
{
    public class VocabularyRepository : IVocabularyRepository
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        public VocabularyRepository(AppDbContext context)
        {
            _context = context;
        }

        // get total count of vocabulary
        public async Task<int> CountAsync()
        {
            return await _context.Vocabularies.CountAsync();
        }

        // get random word
        public async Task<Vocabulary> GetRandomAsync()
        {
            var count = await _context.Vocabularies.CountAsync(); // get total count of vocabulary
            if (count == 0) return null; // if no vocabulary, return null
            // get random skip count
            var skipCount = _random.Next(count);
            // get random word by skip count and take 1
            return await _context.Vocabularies
                .Skip(skipCount)
                .Take(1)
                .FirstOrDefaultAsync();
        }
    }
}


