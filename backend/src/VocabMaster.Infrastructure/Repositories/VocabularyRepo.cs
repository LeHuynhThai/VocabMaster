using Microsoft.EntityFrameworkCore;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;
using VocabMaster.Infrastructure.Persistence;

namespace VocabMaster.Infrastructure.Repositories
{
    public class VocabularyRepo : IVocabularyRepo
    {
        private readonly ApplicationDbContext _context;

        public VocabularyRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get random word using for get random word exclude learned words
        public async Task<Vocabulary?> GetRandom()
        {
            var count = await _context.Vocabularies.CountAsync();
            if (count == 0)
            {
                return null;
            }

            var skipCount = Random.Shared.Next(count);

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

            if (count == 0)
            {
                return null;
            }

            var skipCount = Random.Shared.Next(count);

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
            if (existingVocabulary == null)
            {
                return false;
            }

            existingVocabulary.Vietnamese = vocabulary.Vietnamese;
            await _context.SaveChangesAsync();
            return true;
        }

        // Get learned words
        public async Task<List<LearnedWord>> GetLearnedWords(int userId)
        {
            var learnedWords = await _context.LearnedVocabularies
                .Where(lw => lw.UserId == userId)
                .ToListAsync();

            return learnedWords;
        }

        // Add learned word
        public async Task<LearnedWord> AddLearnedWord(LearnedWord learnedWord)
        {
            _context.LearnedVocabularies.Add(learnedWord);
            await _context.SaveChangesAsync();
            return learnedWord;
        }

        // Remove learned word
        public async Task<bool> RemoveLearnedWord(int learnedWordId, int userId)
        {
            var learnedWord = await _context.LearnedVocabularies
                .FirstOrDefaultAsync(lw => lw.Id == learnedWordId && lw.UserId == userId);

            if (learnedWord == null)
            {
                return false;
            }

            _context.LearnedVocabularies.Remove(learnedWord);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get vocabulary by word
        public async Task<Vocabulary?> GetVocabularyByWord(string word)
        {
            return await _context.Vocabularies
                .FirstOrDefaultAsync(v => v.Word == word);
        }
    }
}


