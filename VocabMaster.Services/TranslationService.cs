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
            _translationApiUrl = configuration?.GetValue<string>("TranslationApiUrl") ?? "https://libretranslate.de/translate";
            
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
                _logger.LogInformation("Translating text from {SourceLanguage} to {TargetLanguage}: {Text}", sourceLanguage, targetLanguage, text);
                
                // Prepare request data according to LibreTranslate API documentation
                var requestData = new
                {
                    q = text,
                    source = sourceLanguage,
                    target = targetLanguage,
                    api_key = !string.IsNullOrEmpty(_apiKey) ? _apiKey : null,
                    format = "text"
                };
                
                // Serialize request to JSON
                var requestJson = JsonSerializer.Serialize(requestData);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                // Log request details for debugging
                _logger.LogDebug("Sending translation request to {Url} with payload: {Payload}", _translationApiUrl, requestJson);
                
                // Send request to API
                var response = await _httpClient.PostAsync(_translationApiUrl, content);
                
                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log response for debugging
                _logger.LogDebug("Received translation response: {Response}", responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code {StatusCode}: {Response}", 
                        response.StatusCode, responseContent);
                    return new TranslationResponseDto
                    {
                        OriginalText = text,
                        TranslatedText = text, // Return original text if translation fails
                        SourceLanguage = sourceLanguage,
                        TargetLanguage = targetLanguage
                    };
                }
                
                // Parse JSON response
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                try 
                {
                    // Try to parse as standard LibreTranslate response format
                    var translationResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (translationResponse.TryGetProperty("translatedText", out var translatedTextElement))
                    {
                        string translatedText = translatedTextElement.GetString();
                        
                        // Create response DTO
                        var result = new TranslationResponseDto
                        {
                            OriginalText = text,
                            TranslatedText = translatedText ?? text, // Fallback to original if null
                            SourceLanguage = sourceLanguage,
                            TargetLanguage = targetLanguage
                        };
                        
                        _logger.LogInformation("Successfully translated text from {SourceLanguage} to {TargetLanguage}", 
                            sourceLanguage, targetLanguage);
                        return result;
                    }
                    else
                    {
                        _logger.LogWarning("Translation response missing 'translatedText' property: {Response}", responseContent);
                        return new TranslationResponseDto
                        {
                            OriginalText = text,
                            TranslatedText = text,
                            SourceLanguage = sourceLanguage,
                            TargetLanguage = targetLanguage
                        };
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing translation response: {Response}", responseContent);
                    return new TranslationResponseDto
                    {
                        OriginalText = text,
                        TranslatedText = text,
                        SourceLanguage = sourceLanguage,
                        TargetLanguage = targetLanguage
                    };
                }
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