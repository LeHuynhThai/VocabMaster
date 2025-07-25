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
        private readonly string _dictionaryApiUrl;
        
        /// <summary>
        /// Initializes a new instance of the DictionaryService
        /// </summary>
        /// <param name="logger">Logger for the service</param>
        /// <param name="vocabularyRepository">Repository for vocabulary operations</param>
        /// <param name="learnedWordRepository">Repository for learned words operations</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="httpClientFactory">Factory for creating HttpClient instances</param>
        public DictionaryService(
            ILogger<DictionaryService> logger,
            IVocabularyRepo vocabularyRepository,
            ILearnedWordRepo learnedWordRepository,
            IConfiguration configuration = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            
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
                return await GetWordDefinition(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }
        
        /// <summary>
        /// Gets a random word excluding words already learned by the user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Dictionary response with word details or null if not found</returns>
        public async Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId)
        {
            try
            {
                _logger.LogInformation("Getting learned words for user {UserId}", userId);
                var learnedVocabularies = await _learnedWordRepository.GetByUserId(userId);
                
                if (learnedVocabularies == null)
                {
                    _logger.LogWarning("No learned vocabularies found for user {UserId}", userId);
                    return await GetRandomWord();
                }
                
                var learnedWords = learnedVocabularies.Select(lv => lv.Word).ToList();
                _logger.LogInformation("User {UserId} has learned {Count} words", userId, learnedWords.Count);
                
                var vocabulary = await _vocabularyRepository.GetRandomExcludeLearned(learnedWords);
                
                if (vocabulary == null)
                {
                    _logger.LogInformation("No unlearned words found for user {UserId}", userId);
                    return null;
                }
                
                _logger.LogInformation("Found random unlearned word: {Word}", vocabulary.Word);
                return await GetWordDefinition(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word excluding learned words for user {UserId}", userId);
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
    }
}
