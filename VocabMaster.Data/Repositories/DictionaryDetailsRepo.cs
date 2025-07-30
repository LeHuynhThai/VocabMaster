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
        /// <returns>Dictionary details or null if not found</returns>
        public async Task<DictionaryDetails> GetByWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Getting dictionary details for word: {Word}", word);
                return await _context.DictionaryDetails
                    .FirstOrDefaultAsync(dd => dd.Word.ToLower() == word.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dictionary details for word: {Word}", word);
                return null;
            }
        }

        /// <summary>
        /// Adds or updates a dictionary details entity
        /// </summary>
        /// <param name="details">The dictionary details to add or update</param>
        /// <returns>The added or updated entity</returns>
        public async Task<DictionaryDetails> AddOrUpdate(DictionaryDetails details)
        {
            if (details == null)
            {
                _logger.LogWarning("Dictionary details parameter is null");
                return null;
            }

            try
            {
                _logger.LogInformation("Adding or updating dictionary details for word: {Word}", details.Word);

                // Check if the word already exists
                var existingDetails = await _context.DictionaryDetails
                    .FirstOrDefaultAsync(dd => dd.Word.ToLower() == details.Word.ToLower());

                if (existingDetails != null)
                {
                    _logger.LogInformation("Updating existing dictionary details for word: {Word}", details.Word);

                    // Update existing entry
                    existingDetails.PhoneticsJson = details.PhoneticsJson;
                    existingDetails.MeaningsJson = details.MeaningsJson;
                    existingDetails.UpdatedAt = DateTime.UtcNow;

                    _context.DictionaryDetails.Update(existingDetails);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully updated dictionary details for word: {Word}", details.Word);
                    return existingDetails;
                }
                else
                {
                    _logger.LogInformation("Adding new dictionary details for word: {Word}", details.Word);

                    // Add new entry
                    await _context.DictionaryDetails.AddAsync(details);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully added dictionary details for word: {Word}", details.Word);
                    return details;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or updating dictionary details for word: {Word}", details.Word);
                return null;
            }
        }

        /// <summary>
        /// Gets all dictionary details
        /// </summary>
        /// <returns>List of all dictionary details</returns>
        public async Task<List<DictionaryDetails>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all dictionary details");
                return await _context.DictionaryDetails.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all dictionary details");
                return new List<DictionaryDetails>();
            }
        }

        /// <summary>
        /// Checks if dictionary details exist for a word
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
                _logger.LogInformation("Checking if dictionary details exist for word: {Word}", word);
                return await _context.DictionaryDetails
                    .AnyAsync(dd => dd.Word.ToLower() == word.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if dictionary details exist for word: {Word}", word);
                return false;
            }
        }
    }
}