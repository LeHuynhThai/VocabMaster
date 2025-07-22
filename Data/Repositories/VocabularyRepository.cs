using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
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
        public async Task<int> Count()
        {
            return await _context.Vocabularies.CountAsync();
        }

        // get random word
        public async Task<Vocabulary> GetRandom()
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
        
        // get random word excluding learned words
        public async Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords)
        {
            // If no learned words, just return a random word
            if (learnedWords == null || !learnedWords.Any())
            {
                return await GetRandom();
            }
            
            // Get all vocabulary words that are not in the learned words list
            var availableWords = await _context.Vocabularies
                .Where(v => !learnedWords.Contains(v.Word))
                .ToListAsync();
                
            // If no available words, return null
            if (availableWords.Count == 0)
            {
                return null;
            }
            
            // Get a random word from the available words
            var randomIndex = _random.Next(availableWords.Count);
            return availableWords[randomIndex];
        }
    }
}


