// VocabMaster.Services/Dictionary/DictionaryLookupService.cs
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
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
        private const string API_URL = "https://api.dictionaryapi.dev/api/v2/entries/en/";

        public DictionaryLookupService(
            ILogger<DictionaryLookupService> logger,
            IVocabularyRepo vocabularyRepository,
            IDictionaryDetailsRepo dictionaryDetailsRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _dictionaryDetailsRepository = dictionaryDetailsRepository ?? throw new ArgumentNullException(nameof(dictionaryDetailsRepository));
        }

        public async Task<DictionaryResponseDto> GetWordDefinition(string word)
        {
            var result = await GetWordDefinitionFromDatabase(word);
            
            if (result == null)
            {
                _logger.LogInformation("Word '{Word}' not found in database, getting from API", word);
                
                result = await GetWordDefinitionFromApi(word);
                
                if (result != null)
                {
                    await SaveWordDefinitionToDatabase(result);
                }
            }
            
            return result;
        }

        public async Task<DictionaryResponseDto> GetWordDefinitionFromDatabase(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word to lookup is empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Getting data from database for word: {Word}", word);
                var vocabulary = await _dictionaryDetailsRepository.GetByWord(word);

                string vietnameseTranslation = vocabulary?.Vietnamese;

                _logger.LogInformation("Vietnamese translation for word {Word}: {Translation}", 
                    word, vietnameseTranslation ?? "no translation");

                if (vocabulary == null)
                {
                    _logger.LogInformation("Word {Word} not found in database", word);
                    return null;
                }

                _logger.LogInformation("Found word: {Word} in database", word);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var phonetics = string.IsNullOrEmpty(vocabulary.PhoneticsJson)
                    ? new List<Phonetic>()
                    : JsonSerializer.Deserialize<List<Phonetic>>(vocabulary.PhoneticsJson, options);

                var meanings = string.IsNullOrEmpty(vocabulary.MeaningsJson)
                    ? new List<Meaning>()
                    : JsonSerializer.Deserialize<List<Meaning>>(vocabulary.MeaningsJson, options);

                var response = new DictionaryResponseDto
                {
                    Word = vocabulary.Word,
                    Phonetic = phonetics.FirstOrDefault()?.Text ?? "",
                    Phonetics = phonetics,
                    Meanings = meanings,
                    Vietnamese = vietnameseTranslation
                };

                response.PhoneticsJson = vocabulary.PhoneticsJson;
                response.MeaningsJson = vocabulary.MeaningsJson;

                _logger.LogInformation("Successfully retrieved data for word: {Word}", word);
                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for word: {Word}", word);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data for word: {Word}", word);
                return null;
            }
        }

        private async Task<DictionaryResponseDto> GetWordDefinitionFromApi(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word to lookup is empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Calling dictionary API for word: {Word}", word);
                
                using (var httpClient = new HttpClient())
                {
                    string apiUrl = $"{API_URL}{Uri.EscapeDataString(word)}";
                    var response = await httpClient.GetAsync(apiUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("API returned status code {StatusCode} for word {Word}", 
                            response.StatusCode, word);
                        return null;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponseDto>>(content, options);
                    
                    var result = dictionaryResponse?.FirstOrDefault();

                    if (result == null)
                    {
                        _logger.LogWarning("No definition found from API for word: {Word}", word);
                        return null;
                    }

                    _logger.LogInformation("Successfully retrieved definition from API for word: {Word}", word);
                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error when calling API for word: {Word}", word);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error from API response for word: {Word}", word);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API for word: {Word}", word);
                return null;
            }
        }

        private async Task<bool> SaveWordDefinitionToDatabase(DictionaryResponseDto definition)
        {
            if (definition == null)
            {
                _logger.LogWarning("Definition is null, cannot save to database");
                return false;
            }

            try
            {
                _logger.LogInformation("Saving definition for word: {Word} to database", definition.Word);
                
                var existingVocabulary = await _dictionaryDetailsRepository.GetByWord(definition.Word);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                var phoneticsJson = definition.Phonetics != null && definition.Phonetics.Any()
                    ? JsonSerializer.Serialize(definition.Phonetics, options)
                    : "[]";

                var meaningsJson = definition.Meanings != null 
                    ? JsonSerializer.Serialize(definition.Meanings, options)
                    : "[]";

                if (existingVocabulary != null)
                {
                    existingVocabulary.PhoneticsJson = phoneticsJson;
                    existingVocabulary.MeaningsJson = meaningsJson;
                    existingVocabulary.UpdatedAt = DateTime.UtcNow;
                    
                    if (!string.IsNullOrEmpty(definition.Vietnamese))
                    {
                        existingVocabulary.Vietnamese = definition.Vietnamese;
                    }
                    
                    await _dictionaryDetailsRepository.AddOrUpdate(existingVocabulary);
                    
                    _logger.LogInformation("Updated existing word: {Word} in database", definition.Word);
                }
                else
                {
                    var newVocabulary = new VocabMaster.Core.Entities.Vocabulary
                    {
                        Word = definition.Word,
                        PhoneticsJson = phoneticsJson,
                        MeaningsJson = meaningsJson,
                        Vietnamese = definition.Vietnamese,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _dictionaryDetailsRepository.AddOrUpdate(newVocabulary);
                    
                    _logger.LogInformation("Added new word: {Word} to database", definition.Word);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving definition for word: {Word} to database", definition.Word);
                return false;
            }
        }
    }
}