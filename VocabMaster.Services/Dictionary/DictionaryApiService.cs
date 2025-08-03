using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.Services.Dictionary
{
    public class DictionaryApiService : IDictionaryApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryApiService> _logger;
        private readonly string _dictionaryApiUrl;

        public DictionaryApiService(
            ILogger<DictionaryApiService> logger,
            IConfiguration configuration = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Use IHttpClientFactory if provided, otherwise create a new HttpClient
            _httpClient = httpClientFactory != null ? httpClientFactory.CreateClient("DictionaryApi") : new HttpClient();

            // Get API URL from configuration if provided, otherwise use default
            _dictionaryApiUrl = configuration?.GetValue<string>("DictionaryApiUrl") ?? "https://api.dictionaryapi.dev/api/v2/entries/en/";
        }

        public async Task<DictionaryResponseDto> GetWordDefinitionFromApi(string word)
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