using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.Services.Dictionary
{
    public class DictionaryCacheService : IDictionaryCacheService
    {
        private readonly ILogger<DictionaryCacheService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly IDictionaryDetailsRepo _dictionaryDetailsRepository;

        public DictionaryCacheService(
            ILogger<DictionaryCacheService> logger,
            IVocabularyRepo vocabularyRepository,
            IDictionaryDetailsRepo dictionaryDetailsRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _dictionaryDetailsRepository = dictionaryDetailsRepository ?? throw new ArgumentNullException(nameof(dictionaryDetailsRepository));
        }

        public async Task<DictionaryDetails> CacheDefinition(DictionaryResponseDto definition)
        {
            if (definition == null)
            {
                _logger.LogWarning("Definition parameter is null");
                return null;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                // Serialize the phonetics and meanings
                var phoneticsJson = definition.Phonetics != null && definition.Phonetics.Any()
                    ? JsonSerializer.Serialize(definition.Phonetics, options)
                    : "[]";

                var meaningsJson = JsonSerializer.Serialize(definition.Meanings ?? new List<Meaning>(), options);

                // Create the dictionary details entity
                var dictionaryDetails = new DictionaryDetails
                {
                    Word = definition.Word,
                    PhoneticsJson = phoneticsJson,
                    MeaningsJson = meaningsJson,
                    CreatedAt = DateTime.UtcNow
                };

                // Save to the repository
                var result = await _dictionaryDetailsRepository.AddOrUpdate(dictionaryDetails);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching definition for word: {Word}", definition.Word);
                return null;
            }
        }

        public async Task<bool> CacheWordDefinition(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return false;
            }

            try
            {
                _logger.LogInformation("Caching definition for word: {Word}", word);

                // Check if already cached
                var exists = await _dictionaryDetailsRepository.Exists(word);
                if (exists)
                {
                    _logger.LogInformation("Word {Word} is already cached", word);
                    return true;
                }

                // Không thể gọi DictionaryLookupService vì sẽ tạo ra circular dependency
                // Trả về false để biểu thị rằng không thể cache từ này
                _logger.LogWarning("Cannot cache word definition due to service architecture constraints");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching definition for word: {Word}", word);
                return false;
            }
        }

        public async Task<int> CacheAllVocabularyDefinitions()
        {
            try
            {
                _logger.LogInformation("Starting to cache all vocabulary definitions");

                // Get all vocabularies
                var vocabularies = await _vocabularyRepository.GetAll();
                if (vocabularies == null || !vocabularies.Any())
                {
                    _logger.LogWarning("No vocabularies found to cache");
                    return 0;
                }

                _logger.LogInformation("Found {Count} vocabularies to cache", vocabularies.Count);

                // Không thể cache tất cả từ vì sẽ tạo ra circular dependency
                // Trả về 0 để biểu thị rằng không có từ nào được cache
                _logger.LogWarning("Cannot cache all vocabulary definitions due to service architecture constraints");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching all vocabulary definitions");
                return 0;
            }
        }
    }
}