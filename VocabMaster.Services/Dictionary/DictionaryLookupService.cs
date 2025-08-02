using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.Services.Dictionary
{
    public class DictionaryLookupService : IDictionaryLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryLookupService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly IDictionaryDetailsRepo _dictionaryDetailsRepository;
        private readonly IDictionaryCacheService _dictionaryCacheService;
        private readonly string _dictionaryApiUrl;

        public DictionaryLookupService(
            ILogger<DictionaryLookupService> logger,
            IVocabularyRepo vocabularyRepository,
            IDictionaryDetailsRepo dictionaryDetailsRepository,
            IDictionaryCacheService dictionaryCacheService,
            IConfiguration configuration = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _dictionaryDetailsRepository = dictionaryDetailsRepository ?? throw new ArgumentNullException(nameof(dictionaryDetailsRepository));
            _dictionaryCacheService = dictionaryCacheService ?? throw new ArgumentNullException(nameof(dictionaryCacheService));

            // Use IHttpClientFactory if provided, otherwise create a new HttpClient
            _httpClient = httpClientFactory != null ? httpClientFactory.CreateClient("DictionaryApi") : new HttpClient();

            // Get API URL from configuration if provided, otherwise use default
            _dictionaryApiUrl = configuration?.GetValue<string>("DictionaryApiUrl") ?? "https://api.dictionaryapi.dev/api/v2/entries/en/";
        }

        public async Task<DictionaryResponseDto> GetWordDefinition(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Looking up definition for word: {Word}", word);
                var requestUri = $"{_dictionaryApiUrl}{Uri.EscapeDataString(word)}";

                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code {StatusCode} for word {Word}",
                        response.StatusCode, word);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponseDto>>(content, options);

                var result = dictionaryResponse?.FirstOrDefault();

                if (result == null)
                {
                    _logger.LogWarning("No definition found for word: {Word}", word);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved definition for word: {Word}", word);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while getting definition for word: {Word}", word);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for word: {Word}", word);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting definition for word: {Word}", word);
                return null;
            }
        }

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
                    var definition = await GetWordDefinition(word);

                    // Cache the definition if found
                    if (definition != null)
                    {
                        _logger.LogInformation("Caching definition for word: {Word}", word);
                        await _dictionaryCacheService.CacheDefinition(definition);

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