// VocabMaster.Services/Dictionary/DictionaryLookupService.cs
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Dictionary;
using VocabMaster.Core.Entities;

namespace VocabMaster.Services.Dictionary
{
    public class DictionaryLookupService : IDictionaryLookupService
    {
        private readonly ILogger<DictionaryLookupService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly IDictionaryDetailsRepo _dictionaryDetailsRepository;
        private readonly IDictionaryApiService _dictionaryApiService;

        public DictionaryLookupService(
            ILogger<DictionaryLookupService> logger,
            IVocabularyRepo vocabularyRepository,
            IDictionaryDetailsRepo dictionaryDetailsRepository,
            IDictionaryApiService dictionaryApiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _dictionaryDetailsRepository = dictionaryDetailsRepository ?? throw new ArgumentNullException(nameof(dictionaryDetailsRepository));
            _dictionaryApiService = dictionaryApiService ?? throw new ArgumentNullException(nameof(dictionaryApiService));
        }

        // Get word definition directly from API
        public async Task<DictionaryResponseDto> GetWordDefinition(string word)
        {
            return await _dictionaryApiService.GetWordDefinitionFromApi(word);
        }

        // Get word definition from cache or API if not cached
        public async Task<DictionaryResponseDto> GetWordDefinitionFromCache(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Getting dictionary details from cache for word: {Word}", word);
                var dictionaryDetails = await _dictionaryDetailsRepository.GetByWord(word);

                // Get the Vietnamese translation from the vocabulary table
                var vocabulary = (await _vocabularyRepository.GetAll()).FirstOrDefault(v => v.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
                string vietnameseTranslation = vocabulary?.Vietnamese;

                // Debug Vietnamese translation
                _logger.LogInformation("Vietnamese translation for word {Word}: {Translation}", word, vietnameseTranslation ?? "null");

                if (dictionaryDetails == null)
                {
                    _logger.LogInformation("No cached details found for word: {Word}, getting from API", word);

                    // Get from API if not in cache
                    var definition = await _dictionaryApiService.GetWordDefinitionFromApi(word);

                    // Cache the definition if found
                    if (definition != null)
                    {
                        _logger.LogInformation("Caching definition for word: {Word}", word);
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
                        var dictionaryDetailsEntity = new DictionaryDetails
                        {
                            Word = definition.Word,
                            PhoneticsJson = phoneticsJson,
                            MeaningsJson = meaningsJson,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Save to the repository
                        await _dictionaryDetailsRepository.AddOrUpdate(dictionaryDetailsEntity);

                        // Add Vietnamese translation if available
                        if (!string.IsNullOrEmpty(vietnameseTranslation))
                        {
                            _logger.LogInformation("Adding Vietnamese translation to definition: {Translation}", vietnameseTranslation);
                            definition.Vietnamese = vietnameseTranslation;
                        }
                        else
                        {
                            _logger.LogWarning("No Vietnamese translation available for word: {Word}", word);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No definition found from API for word: {Word}", word);
                    }

                    return definition;
                }

                _logger.LogInformation("Found cached definition for word: {Word}, using database data", word);

                // Deserialize the JSON data
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var phonetics = string.IsNullOrEmpty(dictionaryDetails.PhoneticsJson)
                    ? new List<Phonetic>()
                    : JsonSerializer.Deserialize<List<Phonetic>>(dictionaryDetails.PhoneticsJson, options);

                var meanings = JsonSerializer.Deserialize<List<Meaning>>(dictionaryDetails.MeaningsJson, options);

                // Create and return the dictionary response
                var response = new DictionaryResponseDto
                {
                    Word = dictionaryDetails.Word,
                    Phonetic = phonetics.FirstOrDefault()?.Text ?? "",
                    Phonetics = phonetics,
                    Meanings = meanings,
                    Vietnamese = vietnameseTranslation
                };

                // Debug final response
                _logger.LogInformation("Final response for word {Word} has Vietnamese: {HasVietnamese}",
                    word, !string.IsNullOrEmpty(response.Vietnamese) ? "Yes" : "No");

                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for cached word: {Word}", word);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting cached definition for word: {Word}", word);
                return null;
            }
        }
    }
}