using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    /// <summary>
    /// Repository implementation for dictionary details operations
    /// </summary>
    public class DictionaryDetailsRepo : IDictionaryDetailsRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DictionaryDetailsRepo> _logger;

        /// <summary>
        /// Initializes a new instance of the DictionaryDetailsRepo
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logger">Logger for the repository</param>
        public DictionaryDetailsRepo(AppDbContext context, ILogger<DictionaryDetailsRepo> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets dictionary details by word
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Vocabulary or null if not found</returns>
        public async Task<Vocabulary> GetByWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Getting vocabulary details for word: {Word}", word);
                return await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == word.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vocabulary details for word: {Word}", word);
                return null;
            }
        }

        /// <summary>
        /// Adds or updates a vocabulary entity
        /// </summary>
        /// <param name="details">The vocabulary to add or update</param>
        /// <returns>The added or updated entity</returns>
        public async Task<Vocabulary> AddOrUpdate(Vocabulary details)
        {
            if (details == null)
            {
                _logger.LogWarning("Vocabulary details parameter is null");
                return null;
            }

            try
            {
                _logger.LogInformation("Adding or updating vocabulary details for word: {Word}", details.Word);

                // Check if the word already exists
                var existingDetails = await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == details.Word.ToLower());

                if (existingDetails != null)
                {
                    _logger.LogInformation("Updating existing vocabulary details for word: {Word}", details.Word);

                    // Update existing entry
                    existingDetails.PhoneticsJson = details.PhoneticsJson;
                    existingDetails.MeaningsJson = details.MeaningsJson;
                    existingDetails.UpdatedAt = DateTime.UtcNow;

                    _context.Vocabularies.Update(existingDetails);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully updated vocabulary details for word: {Word}", details.Word);
                    return existingDetails;
                }
                else
                {
                    _logger.LogInformation("Adding new vocabulary details for word: {Word}", details.Word);

                    // Add new entry
                    await _context.Vocabularies.AddAsync(details);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully added vocabulary details for word: {Word}", details.Word);
                    return details;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or updating vocabulary details for word: {Word}", details.Word);
                return null;
            }
        }

        /// <summary>
        /// Gets all vocabulary entries
        /// </summary>
        /// <returns>List of all vocabulary entries</returns>
        public async Task<List<Vocabulary>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all vocabulary details");
                return await _context.Vocabularies.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vocabulary details");
                return new List<Vocabulary>();
            }
        }

        /// <summary>
        /// Checks if vocabulary exists for a word
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <returns>True if details exist, false otherwise</returns>
        public async Task<bool> Exists(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return false;
            }

            try
            {
                _logger.LogInformation("Checking if vocabulary exists for word: {Word}", word);
                return await _context.Vocabularies
                    .AnyAsync(v => v.Word.ToLower() == word.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if vocabulary exists for word: {Word}", word);
                return false;
            }
        }
    }
}