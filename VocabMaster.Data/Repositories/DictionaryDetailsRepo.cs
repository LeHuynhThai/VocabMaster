using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    public class DictionaryDetailsRepo : IDictionaryDetailsRepo
    {
        private readonly AppDbContext _context; 
        private readonly ILogger<DictionaryDetailsRepo> _logger;

        public DictionaryDetailsRepo(AppDbContext context, ILogger<DictionaryDetailsRepo> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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

                var existingDetails = await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == details.Word.ToLower());

                if (existingDetails != null)
                {
                    _logger.LogInformation("Updating existing vocabulary details for word: {Word}", details.Word);

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