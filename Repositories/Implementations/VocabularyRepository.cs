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

        public async Task<int> CountAsync()
        {
            return await _context.Vocabularies.CountAsync();
        }

        public async Task<Vocabulary> GetRandomAsync()
        {
            var count = await _context.Vocabularies.CountAsync();
            if (count == 0) return null;

            var skipCount = _random.Next(count);
            return await _context.Vocabularies
                .Skip(skipCount)
                .Take(1)
                .FirstOrDefaultAsync();
        }
    }
}


