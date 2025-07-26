using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Services
{
    /// <summary>
    /// Service for dictionary operations including word lookup and random word generation
    /// </summary>
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly ITranslationService _translationService;
        private readonly string _dictionaryApiUrl;
        private readonly Random _random = new Random();
        private const int MAX_ATTEMPTS = 5;
        
        /// <summary>
        /// Initializes a new instance of the DictionaryService
        /// </summary>
        /// <param name="logger">Logger for the service</param>
        /// <param name="vocabularyRepository">Repository for vocabulary operations</param>
        /// <param name="learnedWordRepository">Repository for learned words operations</param>
        /// <param name="translationService">Service for translation operations</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="httpClientFactory">Factory for creating HttpClient instances</param>
        public DictionaryService(
            ILogger<DictionaryService> logger,
            IVocabularyRepo vocabularyRepository,
            ILearnedWordRepo learnedWordRepository,
            ITranslationService translationService,
            IConfiguration configuration = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            
            // Use IHttpClientFactory if provided, otherwise create a new HttpClient
            _httpClient = httpClientFactory != null ? httpClientFactory.CreateClient("DictionaryApi") : new HttpClient();
            
            // Get API URL from configuration if provided, otherwise use default
            _dictionaryApiUrl = configuration?.GetValue<string>("DictionaryApiUrl") ?? "https://api.dictionaryapi.dev/api/v2/entries/en/";
        }

        /// <summary>
        /// Gets a random word and its definition
        /// </summary>
        /// <returns>Dictionary response with word details or null if not found</returns>
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
                return await GetWordDefinitionWithTranslation(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }
        
        /// <summary>
        /// Gets a random word, optionally trying to exclude words already learned by the user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Dictionary response with word details or null if not found</returns>
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
        
        /// <summary>
        /// Helper method to try getting a random word excluding learned ones
        /// </summary>
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
                            return await GetWordDefinitionWithTranslation(randomVocab.Word);
                        }
                    }
                    
                    _logger.LogInformation("All attempts to find unlearned word failed, returning any random word");
                    return await GetRandomWord();
                }
                
                _logger.LogInformation("Found random unlearned word: {Word}", vocabulary.Word);
                return await GetWordDefinitionWithTranslation(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TryGetRandomWordExcludeLearned for user {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Gets the definition of a word from the dictionary API
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Dictionary response with word details or null if not found</returns>
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

        /// <summary>
        /// Gets the definition of a word and translates relevant parts to Vietnamese
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>Dictionary response with translated details or null if not found</returns>
        public async Task<DictionaryResponseDto> GetWordDefinitionWithTranslation(string word)
        {
            var definition = await GetWordDefinition(word);
            if (definition == null)
            {
                return null;
            }

            try
            {
                // Translate the word itself
                var wordTranslation = await _translationService.TranslateEnglishToVietnamese(definition.Word);
                
                // Process each meaning
                foreach (var meaning in definition.Meanings)
                {
                    // Translate part of speech
                    var partOfSpeechTranslation = await _translationService.TranslateEnglishToVietnamese(meaning.PartOfSpeech);
                    meaning.PartOfSpeech += $" ({partOfSpeechTranslation.TranslatedText})";
                    
                    // Process each definition in this meaning
                    foreach (var def in meaning.Definitions)
                    {
                        // Translate definition text
                        var definitionTranslation = await _translationService.TranslateEnglishToVietnamese(def.Text);
                        def.Text += $"\n→ {definitionTranslation.TranslatedText}";
                        
                        // Translate example if present
                        if (!string.IsNullOrEmpty(def.Example))
                        {
                            var exampleTranslation = await _translationService.TranslateEnglishToVietnamese(def.Example);
                            def.Example += $"\n→ {exampleTranslation.TranslatedText}";
                        }
                    }
                }
                
                _logger.LogInformation("Successfully translated definition for word: {Word}", word);
                return definition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating definition for word: {Word}", word);
                // Return the original untranslated definition if translation fails
                return definition;
            }
        }
    }
}
