using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VocabMaster.Core.Interfaces.Services.Translation;

namespace VocabMaster.Services.Translation
{
    public class TranslationApiService : ITranslationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TranslationApiService> _logger;
        private readonly string _translationApiUrl;

        public TranslationApiService(
            ILogger<TranslationApiService> logger,
            IConfiguration configuration = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Use IHttpClientFactory if provided, otherwise create a new HttpClient
            _httpClient = httpClientFactory != null ? httpClientFactory.CreateClient("TranslationApi") : new HttpClient();

            // Get API URL from configuration if provided, otherwise use default
            _translationApiUrl = configuration?.GetValue<string>("TranslationApiUrl") ?? "https://lingvanex.com/api/v1/translate";
        }

        public async Task<string> TranslateWordViaApi(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Translating word via API: {Word}", word);

                // Try to use Google Translate API
                // Note: This is using a free endpoint that might not be stable long-term
                string googleTranslateUrl = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q={Uri.EscapeDataString(word)}";

                var response = await _httpClient.GetAsync(googleTranslateUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parse the response
                    // Google Translate API returns a nested array structure
                    // The first element of the first array contains the translation
                    try
                    {
                        // Using System.Text.Json to parse the response
                        using (JsonDocument document = JsonDocument.Parse(responseContent))
                        {
                            var root = document.RootElement;

                            // Navigate to the first translation
                            if (root.ValueKind == JsonValueKind.Array &&
                                root[0].ValueKind == JsonValueKind.Array &&
                                root[0][0].ValueKind == JsonValueKind.Array &&
                                root[0][0][0].ValueKind == JsonValueKind.String)
                            {
                                string translation = root[0][0][0].GetString();

                                if (!string.IsNullOrEmpty(translation))
                                {
                                    _logger.LogInformation("Successfully translated word via API: {Word} to {Translation}", word, translation);
                                    return translation;
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error parsing Google Translate response for word: {Word}", word);
                    }
                }

                _logger.LogWarning("API translation failed for word: {Word}", word);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while translating word: {Word}", word);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error translating word via API: {Word}", word);
                return null;
            }
        }
    }
} 