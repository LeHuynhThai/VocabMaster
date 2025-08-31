using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    public class VocabRepo : IVocabularyRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VocabRepo> _logger;
        private readonly Random _random;

        public VocabRepo(
            AppDbContext context,
            ILogger<VocabRepo> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            _random = new Random();
        }

        public async Task<int> Count()
        {
            _logger?.LogInformation("Getting total count of vocabularies");
            return await _context.Vocabularies.CountAsync();
        }

        public async Task<Vocabulary> GetRandom()
        {
            try
            {
                var count = await _context.Vocabularies.CountAsync();

                if (count == 0)
                {
                    _logger?.LogWarning("No vocabularies found in the database");
                    return null;
                }

                _logger?.LogInformation("Found {Count} vocabularies in the database", count);

                var skipCount = _random.Next(count);

                _logger?.LogDebug("Selecting vocabulary at random index: {Index}", skipCount);
                var vocabulary = await _context.Vocabularies
                    .Skip(skipCount)
                    .Take(1)
                    .FirstOrDefaultAsync();

                if (vocabulary == null)
                {
                    _logger?.LogWarning("Failed to retrieve vocabulary at index {Index}", skipCount);
                    return null;
                }

                _logger?.LogInformation("Successfully retrieved random vocabulary: {Word}", vocabulary.Word);
                return vocabulary;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving random vocabulary");
                throw;
            }
        }

        public async Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords)
        {
            try
            {
                if (learnedWords == null || !learnedWords.Any())
                {
                    _logger?.LogInformation("No learned words provided, returning any random word");
                    return await GetRandom();
                }

                _logger?.LogInformation("Finding random word excluding {Count} learned words", learnedWords.Count);

                var availableWords = await _context.Vocabularies
                    .Where(v => !learnedWords.Contains(v.Word))
                    .ToListAsync();

                if (availableWords.Count == 0)
                {
                    _logger?.LogInformation("No unlearned words available - user has learned all words");
                    return null;
                }

                _logger?.LogInformation("Found {Count} unlearned words", availableWords.Count);

                var randomIndex = _random.Next(availableWords.Count);
                var selectedWord = availableWords[randomIndex];

                _logger?.LogInformation("Selected random unlearned word: {Word}", selectedWord.Word);
                return selectedWord;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving random vocabulary excluding learned words");
                throw;
            }
        }

        public async Task<List<Vocabulary>> GetAll()
        {
            try
            {
                _logger?.LogInformation("Getting all vocabularies");
                var vocabularies = await _context.Vocabularies.ToListAsync();
                _logger?.LogInformation("Retrieved {Count} vocabularies", vocabularies.Count);
                return vocabularies;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving all vocabularies");
                throw;
            }
        }

        public async Task<bool> Update(Vocabulary vocabulary)
        {
            try
            {
                if (vocabulary == null)
                {
                    _logger?.LogWarning("Cannot update null vocabulary");
                    return false;
                }

                _logger?.LogInformation("Updating vocabulary: {Word}", vocabulary.Word);

                var existingVocabulary = await _context.Vocabularies.FindAsync(vocabulary.Id);
                if (existingVocabulary == null)
                {
                    _logger?.LogWarning("Vocabulary with ID {Id} not found", vocabulary.Id);
                    return false;
                }

                existingVocabulary.Vietnamese = vocabulary.Vietnamese;

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully updated vocabulary: {Word}", vocabulary.Word);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating vocabulary: {Word}", vocabulary?.Word);
                return false;
            }
        }
    }
}


