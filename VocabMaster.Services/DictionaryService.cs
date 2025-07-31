using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly IDictionaryDetailsRepo _dictionaryDetailsRepository;
        private readonly string _dictionaryApiUrl;
        private readonly Random _random = new Random();
        private const int MAX_ATTEMPTS = 5;

        public DictionaryService(
            ILogger<DictionaryService> logger,
            IVocabularyRepo vocabularyRepository,
            ILearnedWordRepo learnedWordRepository,
            IDictionaryDetailsRepo dictionaryDetailsRepository,
            IConfiguration configuration = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _dictionaryDetailsRepository = dictionaryDetailsRepository ?? throw new ArgumentNullException(nameof(dictionaryDetailsRepository));

            // Use IHttpClientFactory if provided, otherwise create a new HttpClient
            _httpClient = httpClientFactory != null ? httpClientFactory.CreateClient("DictionaryApi") : new HttpClient();

            // Get API URL from configuration if provided, otherwise use default
            _dictionaryApiUrl = configuration?.GetValue<string>("DictionaryApiUrl") ?? "https://api.dictionaryapi.dev/api/v2/entries/en/";
        }

        public async Task<DictionaryResponseDto> GetRandomWord()
        {
            try
            {
                _logger.LogInformation("Getting random word from vocabulary repository");
                var vocabulary = await _vocabularyRepository.GetRandom();

                if (vocabulary == null)
                {
                    _logger.LogWarning("No vocabulary found in the repository");
                    return null;
                }

                _logger.LogInformation("Found random word: {Word}", vocabulary.Word);
                return await GetWordDefinitionFromCache(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }

        public async Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId)
        {
            try
            {
                // First try to get a word excluding learned ones
                var result = await TryGetRandomWordExcludeLearned(userId);

                // If no unlearned word is found or there's an issue, fall back to any random word
                if (result == null)
                {
                    _logger.LogInformation("Falling back to any random word for user {UserId}", userId);
                    return await GetRandomWord();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRandomWordExcludeLearned for user {UserId}, falling back to any random word", userId);
                // In case of any error, still try to return a random word
                return await GetRandomWord();
            }
        }

        private async Task<DictionaryResponseDto> TryGetRandomWordExcludeLearned(int userId)
        {
            try
            {
                _logger.LogInformation("Getting learned words for user {UserId}", userId);
                var learnedVocabularies = await _learnedWordRepository.GetByUserId(userId);

                if (learnedVocabularies == null || !learnedVocabularies.Any())
                {
                    _logger.LogInformation("No learned vocabularies found for user {UserId}, returning any random word", userId);
                    return await GetRandomWord();
                }

                var learnedWords = learnedVocabularies.Select(lv => lv.Word.ToLowerInvariant()).ToHashSet();
                _logger.LogInformation("User {UserId} has learned {Count} words", userId, learnedWords.Count);

                // Try to get an unlearned word
                var vocabulary = await _vocabularyRepository.GetRandomExcludeLearned(learnedWords.ToList());

                if (vocabulary == null)
                {
                    _logger.LogInformation("No unlearned words found for user {UserId} in the first attempt", userId);

                    // If all words from the repository are learned, try multiple random words
                    // until we find one that's not in the learned list
                    for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
                    {
                        var randomVocab = await _vocabularyRepository.GetRandom();
                        if (randomVocab != null && !learnedWords.Contains(randomVocab.Word.ToLowerInvariant()))
                        {
                            _logger.LogInformation("Found unlearned random word on attempt {Attempt}: {Word}", attempt + 1, randomVocab.Word);
                            return await GetWordDefinitionFromCache(randomVocab.Word);
                        }
                    }

                    _logger.LogInformation("All attempts to find unlearned word failed, returning any random word");
                    return await GetRandomWord();
                }

                _logger.LogInformation("Found random unlearned word: {Word}", vocabulary.Word);
                return await GetWordDefinitionFromCache(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TryGetRandomWordExcludeLearned for user {UserId}", userId);
                return null;
            }
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
                        await CacheDefinition(definition);

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

                // Get definition from API
                var definition = await GetWordDefinition(word);
                if (definition == null)
                {
                    _logger.LogWarning("Could not get definition for word: {Word}", word);
                    return false;
                }

                // Cache the definition
                var result = await CacheDefinition(definition);
                return result != null;
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

                int successCount = 0;
                int failCount = 0;

                // Process each vocabulary
                foreach (var vocabulary in vocabularies)
                {
                    try
                    {
                        // Check if already cached
                        var exists = await _dictionaryDetailsRepository.Exists(vocabulary.Word);
                        if (exists)
                        {
                            _logger.LogInformation("Word {Word} is already cached, skipping", vocabulary.Word);
                            successCount++;
                            continue;
                        }

                        // Get definition from API
                        var definition = await GetWordDefinition(vocabulary.Word);
                        if (definition == null)
                        {
                            _logger.LogWarning("Could not get definition for word: {Word}, skipping", vocabulary.Word);
                            failCount++;
                            continue;
                        }

                        // Cache the definition
                        var success = await CacheDefinition(definition);
                        if (success != null) // Changed from success to success != null
                        {
                            successCount++;
                            _logger.LogInformation("Successfully cached definition for word: {Word}", vocabulary.Word);
                        }
                        else
                        {
                            failCount++;
                            _logger.LogWarning("Failed to cache definition for word: {Word}", vocabulary.Word);
                        }

                        // Add a small delay to avoid overloading the API
                        await Task.Delay(500);
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
    
        private async Task<DictionaryDetails> CacheDefinition(DictionaryResponseDto definition)
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
