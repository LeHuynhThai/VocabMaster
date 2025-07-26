using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services
{
    /// <summary>
    /// Service for translation operations using LibreTranslate API
    /// </summary>
    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TranslationService> _logger;
        private readonly string _translationApiUrl;
        private readonly string _apiKey;
        
        /// <summary>
        /// Initializes a new instance of the TranslationService
        /// </summary>
        /// <param name="logger">Logger for the service</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="httpClientFactory">Factory for creating HttpClient instances</param>
        public TranslationService(
            ILogger<TranslationService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Use IHttpClientFactory if provided, otherwise create a new HttpClient
            _httpClient = httpClientFactory != null ? httpClientFactory.CreateClient("TranslationApi") : new HttpClient();
            
            // Get API URL from configuration
            _translationApiUrl = configuration?.GetValue<string>("TranslationApiUrl") ?? "https://libretranslate.com/translate";
            
            // Get API key from configuration (optional)
            _apiKey = configuration?.GetValue<string>("TranslationApiKey");
        }
        
        /// <summary>
        /// Translates text from English to Vietnamese
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <returns>Translation response with translated text</returns>
        public async Task<TranslationResponseDto> TranslateEnglishToVietnamese(string text)
        {
            return await Translate(text, "en", "vi");
        }
        
        /// <summary>
        /// Translates text between specified languages
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="sourceLanguage">Source language code</param>
        /// <param name="targetLanguage">Target language code</param>
        /// <returns>Translation response with translated text</returns>
        public async Task<TranslationResponseDto> Translate(string text, string sourceLanguage, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Text parameter is null or empty");
                return new TranslationResponseDto
                {
                    OriginalText = text,
                    TranslatedText = text,
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage
                };
            }

            try
            {
                _logger.LogInformation("Translating text from {SourceLanguage} to {TargetLanguage}", sourceLanguage, targetLanguage);
                
                // Create request object
                var request = new TranslationRequestDto
                {
                    Q = text,
                    Source = sourceLanguage,
                    Target = targetLanguage
                };
                
                // Add API key if available
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.ApiKey = _apiKey;
                }
                
                // Serialize request to JSON
                var requestJson = JsonSerializer.Serialize(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                // Send request to API
                var response = await _httpClient.PostAsync(_translationApiUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code {StatusCode}", response.StatusCode);
                    return new TranslationResponseDto
                    {
                        OriginalText = text,
                        TranslatedText = text, // Return original text if translation fails
                        SourceLanguage = sourceLanguage,
                        TargetLanguage = targetLanguage
                    };
                }
                
                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Parse JSON response
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                // LibreTranslate returns JSON in format { "translatedText": "..." }
                var translationResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                string translatedText = translationResponse.GetProperty("translatedText").GetString();
                
                // Create response DTO
                var result = new TranslationResponseDto
                {
                    OriginalText = text,
                    TranslatedText = translatedText,
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage
                };
                
                _logger.LogInformation("Successfully translated text from {SourceLanguage} to {TargetLanguage}", sourceLanguage, targetLanguage);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while translating text");
                return new TranslationResponseDto
                {
                    OriginalText = text,
                    TranslatedText = text, // Return original text if translation fails
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON error while processing translation response");
                return new TranslationResponseDto
                {
                    OriginalText = text,
                    TranslatedText = text, // Return original text if translation fails
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during translation");
                return new TranslationResponseDto
                {
                    OriginalText = text,
                    TranslatedText = text, // Return original text if translation fails
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage
                };
            }
        }
    }
} 