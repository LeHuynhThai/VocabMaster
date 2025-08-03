using Microsoft.Extensions.Logging;
using System.Text.Json;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.Services.Dictionary
{
    public class DictionaryCacheService : IDictionaryCacheService
    {
        private readonly ILogger<DictionaryCacheService> _logger;
        private readonly IDictionaryDetailsRepo _dictionaryDetailsRepository;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly IDictionaryApiService _dictionaryApiService;

        public DictionaryCacheService(
            ILogger<DictionaryCacheService> logger,
            IDictionaryDetailsRepo dictionaryDetailsRepository,
            IVocabularyRepo vocabularyRepository,
            IDictionaryApiService dictionaryApiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dictionaryDetailsRepository = dictionaryDetailsRepository ?? throw new ArgumentNullException(nameof(dictionaryDetailsRepository));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _dictionaryApiService = dictionaryApiService ?? throw new ArgumentNullException(nameof(dictionaryApiService));
        }

        // Cache all vocabulary definitions
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

                int successCount = 0;
                int failCount = 0;

                // Process each vocabulary
                foreach (var vocabulary in vocabularies)
                {
                    try
                    {
                        // Check if the word is already cached
                        var exists = await _dictionaryDetailsRepository.Exists(vocabulary.Word);
                        if (exists)
                        {
                            _logger.LogInformation("Word {Word} is already cached", vocabulary.Word);
                            continue;
                        }

                        // Get the definition from the API
                        var definition = await _dictionaryApiService.GetWordDefinitionFromApi(vocabulary.Word);
                        if (definition == null)
                        {
                            _logger.LogWarning("No definition found from API for word: {Word}", vocabulary.Word);
                            failCount++;
                            continue;
                        }

                        // Cache the definition
                        var dictionaryDetails = await CacheDefinitionInternal(definition);
                        
                        if (dictionaryDetails != null)
                        {
                            successCount++;
                            _logger.LogInformation("Successfully cached definition for word: {Word}", vocabulary.Word);
                        }
                        else
                        {
                            failCount++;
                            _logger.LogWarning("Failed to cache definition for word: {Word}", vocabulary.Word);
                        }

                        // Add a delay to avoid overloading the API
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing word: {Word}", vocabulary.Word);
                        failCount++;
                    }
                }

                _logger.LogInformation("Finished caching vocabulary definitions. Success: {SuccessCount}, Failed: {FailCount}",
                    successCount, failCount);

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching all vocabulary definitions");
                return 0;
            }
        }

        // Private helper method for caching a single definition
        private async Task<DictionaryDetails> CacheDefinitionInternal(DictionaryResponseDto definition)
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
    }
}