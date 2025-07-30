using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    /// <summary>
    /// Repository for vocabulary operations
    /// </summary>
    public class VocabRepo : IVocabularyRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VocabRepo> _logger;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the VocabRepo
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logger">Logger for the repository</param>
        public VocabRepo(
            AppDbContext context,
            ILogger<VocabRepo> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            _random = new Random();
        }

        /// <summary>
        /// Gets the total count of vocabularies in the database
        /// </summary>
        /// <returns>Count of vocabularies</returns>
        public async Task<int> Count()
        {
            _logger?.LogInformation("Getting total count of vocabularies");
            return await _context.Vocabularies.CountAsync();
        }

        /// <summary>
        /// Gets a random vocabulary from the database
        /// </summary>
        /// <returns>Random vocabulary or null if none exist</returns>
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

                // Generate a random index
                var skipCount = _random.Next(count);

                _logger?.LogDebug("Selecting vocabulary at random index: {Index}", skipCount);

                // Get the vocabulary at the random index
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

        /// <summary>
        /// Gets multiple random vocabulary items
        /// </summary>
        /// <param name="count">Number of random vocabulary items to get</param>
        /// <returns>List of random vocabulary items</returns>
        public async Task<List<Vocabulary>> GetRandomVocabularies(int count)
        {
            try
            {
                if (count <= 0)
                {
                    _logger?.LogWarning("Invalid count requested: {Count}", count);
                    return new List<Vocabulary>();
                }

                var totalVocabularies = await _context.Vocabularies.CountAsync();

                if (totalVocabularies == 0)
                {
                    _logger?.LogWarning("No vocabularies found in the database");
                    return new List<Vocabulary>();
                }

                _logger?.LogInformation("Retrieving {Count} random vocabularies from {Total} total", count, totalVocabularies);

                // If requested count is greater than available vocabularies, return all
                if (count >= totalVocabularies)
                {
                    _logger?.LogInformation("Requested count exceeds available vocabularies, returning all");
                    return await _context.Vocabularies.ToListAsync();
                }

                // Get all vocabularies and select random ones
                var allVocabularies = await _context.Vocabularies.ToListAsync();
                var result = new List<Vocabulary>();
                var selectedIndices = new HashSet<int>();

                while (result.Count < count)
                {
                    var randomIndex = _random.Next(allVocabularies.Count);
                    
                    // Ensure we don't select the same vocabulary twice
                    if (!selectedIndices.Contains(randomIndex))
                    {
                        selectedIndices.Add(randomIndex);
                        result.Add(allVocabularies[randomIndex]);
                    }
                }

                _logger?.LogInformation("Successfully retrieved {Count} random vocabularies", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving random vocabularies");
                throw;
            }
        }

        /// <summary>
        /// Gets a random vocabulary excluding words that the user has already learned
        /// </summary>
        /// <param name="learnedWords">List of words that the user has already learned</param>
        /// <returns>Random vocabulary excluding learned words, or null if all words have been learned</returns>
        public async Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords)
        {
            try
            {
                // If no learned words provided, return any random word
                if (learnedWords == null || !learnedWords.Any())
                {
                    _logger?.LogInformation("No learned words provided, returning any random word");
                    return await GetRandom();
                }

                _logger?.LogInformation("Finding random word excluding {Count} learned words", learnedWords.Count);

                // Get all vocabularies that are not in the learned words list
                var availableWords = await _context.Vocabularies
                    .Where(v => !learnedWords.Contains(v.Word))
                    .ToListAsync();

                if (availableWords.Count == 0)
                {
                    _logger?.LogInformation("No unlearned words available - user has learned all words");
                    return null;
                }

                _logger?.LogInformation("Found {Count} unlearned words", availableWords.Count);

                // Select a random word from the available words
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

        /// <summary>
        /// Gets all vocabularies
        /// </summary>
        /// <returns>List of all vocabularies</returns>
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

        /// <summary>
        /// Updates a vocabulary in the repository
        /// </summary>
        /// <param name="vocabulary">The vocabulary to update</param>
        /// <returns>True if successful, false otherwise</returns>
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

                // Check if the vocabulary exists
                var existingVocabulary = await _context.Vocabularies.FindAsync(vocabulary.Id);
                if (existingVocabulary == null)
                {
                    _logger?.LogWarning("Vocabulary with ID {Id} not found", vocabulary.Id);
                    return false;
                }

                // Update properties
                existingVocabulary.Vietnamese = vocabulary.Vietnamese;
                // Update other properties as needed

                // Save changes
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


