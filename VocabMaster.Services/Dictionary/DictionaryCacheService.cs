using Microsoft.Extensions.Logging;
using System.Net.Http;
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
        private readonly IVocabularyRepo _vocabularyRepository;
        private const string API_URL = "https://api.dictionaryapi.dev/api/v2/entries/en/";

        public DictionaryCacheService(
            ILogger<DictionaryCacheService> logger,
            IVocabularyRepo vocabularyRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
        }

        public async Task<int> CacheAllVocabularyDefinitions()
        {
            try
            {
                _logger.LogInformation("Start caching all vocabulary");

                // get all vocabularies from database
                var vocabularies = await _vocabularyRepository.GetAll();
                if (vocabularies == null || !vocabularies.Any())
                {
                    _logger.LogWarning("No vocabulary to cache");
                    return 0;
                }

                _logger.LogInformation("Found {Count} vocabulary", vocabularies.Count);

                int successCount = 0;
                int failCount = 0;

                // create new HttpClient for each function call
                using (var httpClient = new HttpClient())
                {
                    // process each vocabulary
                    foreach (var vocab in vocabularies)
                    {
                        try
                        {
                            // check if the word has complete data
                            bool needsUpdate = string.IsNullOrEmpty(vocab.MeaningsJson) || vocab.MeaningsJson == "[]";
                            
                            if (needsUpdate)
                            {
                                // call API dictionary
                                string apiUrl = $"{API_URL}{Uri.EscapeDataString(vocab.Word)}";
                                var response = await httpClient.GetAsync(apiUrl);

                                if (response.IsSuccessStatusCode)
                                {
                                    var content = await response.Content.ReadAsStringAsync();
                                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                    var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponseDto>>(content, options);
                                    var definition = dictionaryResponse?.FirstOrDefault();

                                    if (definition != null)
                                    {
                                        // update data from API
                                        vocab.PhoneticsJson = definition.Phonetics != null && definition.Phonetics.Any()
                                            ? JsonSerializer.Serialize(definition.Phonetics, options)
                                            : "[]";
                                            
                                        vocab.MeaningsJson = definition.Meanings != null
                                            ? JsonSerializer.Serialize(definition.Meanings, options)
                                            : "[]";
                                            
                                        _logger.LogInformation("Successfully got definition from API for word: {Word}", vocab.Word);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("No definition found for word: {Word}", vocab.Word);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("API returned error {StatusCode} for word {Word}", response.StatusCode, vocab.Word);
                                }
                            }
                            
                            // always update metadata
                            vocab.UpdatedAt = DateTime.UtcNow;
                            
                            // save to database
                            var success = await _vocabularyRepository.Update(vocab);
                            
                            if (success)
                            {
                                successCount++;
                                _logger.LogInformation("Successfully cached word: {Word}", vocab.Word);
                            }
                            else
                            {
                                failCount++;
                                _logger.LogWarning("Failed to cache word: {Word}", vocab.Word);
                            }

                            // wait to avoid overloading API
                            if (needsUpdate)
                            {
                                await Task.Delay(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing word: {Word}", vocab.Word);
                            failCount++;
                        }
                    }
                }

                _logger.LogInformation("Finished caching vocabulary. Success: {Success}, Failed: {Fail}", 
                    successCount, failCount);

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching vocabulary");
                return 0;
            }
        }
    }
}