using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    public class VocabRepo : IVocabularyRepo
    {
        private readonly AppDbContext _context;
        private readonly Random _random;

        public VocabRepo(
            AppDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        public async Task<int> Count()
        {
            return await _context.Vocabularies.CountAsync();
        }

        public async Task<Vocabulary> GetRandom()
        {
                var count = await _context.Vocabularies.CountAsync();

                var skipCount = _random.Next(count);

                var vocabulary = await _context.Vocabularies
                    .Skip(skipCount)
                    .Take(1)
                    .FirstOrDefaultAsync();
                return vocabulary;
        }

        public async Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords)
        {
                var availableWords = await _context.Vocabularies
                    .Where(v => !learnedWords.Contains(v.Word))
                    .ToListAsync();
                var randomIndex = _random.Next(availableWords.Count);
                var selectedWord = availableWords[randomIndex];

                return selectedWord;
        }

        public async Task<List<Vocabulary>> GetAll()
        {
                var vocabularies = await _context.Vocabularies.ToListAsync();
                return vocabularies;
        }

        public async Task<bool> Update(Vocabulary vocabulary)
        {
                var existingVocabulary = await _context.Vocabularies.FindAsync(vocabulary.Id);

                existingVocabulary.Vietnamese = vocabulary.Vietnamese;

                await _context.SaveChangesAsync();

                return true;
        }
    }
}


