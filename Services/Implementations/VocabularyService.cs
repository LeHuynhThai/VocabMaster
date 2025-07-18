using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VocabMaster.Entities;
using VocabMaster.Repositories.Interfaces;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations
{
    public class VocabularyService : IVocabularyService
    {
        private readonly ILearnedVocabularyRepository _learnedVocabularyRepository;
        private readonly ILogger<VocabularyService> _logger; // logger for logging errors

        public VocabularyService(
            ILearnedVocabularyRepository learnedVocabularyRepository,
            ILogger<VocabularyService> logger)
        {
            _learnedVocabularyRepository = learnedVocabularyRepository;
            _logger = logger;
        }

        // mark a word as learned for a specific user
        public async Task<bool> MarkWordAsLearnedAsync(int userId, string word)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    _logger.LogWarning("Attempted to mark empty word as learned for user {UserId}", userId);
                    return false;
                }
                // create a new learned vocabulary
                var learnedVocabulary = new LearnedVocabulary
                {
                    UserId = userId,
                    Word = word.Trim()
                };

                return await _learnedVocabularyRepository.AddAsync(learnedVocabulary); // AddAsync method in LearnedVocabularyRepository
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word {Word} as learned for user {UserId}", word, userId);
                return false;
            }
        }

        // get all learned vocabularies for a specific user
        public async Task<List<LearnedVocabulary>> GetUserLearnedVocabulariesAsync(int userId)
        {
            try
            {
                return await _learnedVocabularyRepository.GetByUserIdAsync(userId); // GetByUserIdAsync method in LearnedVocabularyRepository
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned vocabularies for user {UserId}", userId);
                return new List<LearnedVocabulary>();
            }
        }
    }
}


