using System.Text.Json;
using VocabMaster.Models;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        private readonly string _dictionaryApiUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";
        private readonly string _randomWordApiUrl = "https://random-word-api.herokuapp.com/word";
        
        
        public DictionaryService(ILogger<DictionaryService> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<DictionaryResponse> GetRandomWordAsync()
        {
            try
            {
                var randomWordResponse = await _httpClient.GetAsync(_randomWordApiUrl);
                if (!randomWordResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error getting random word: {randomWordResponse.StatusCode}");
                    return null;
                }
                var content = await randomWordResponse.Content.ReadAsStringAsync();
                var words = JsonSerializer.Deserialize<string[]>(content);
                var randomWord = words?.FirstOrDefault();
                return await GetWordDefinitionAsync(randomWord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }

        public async Task<DictionaryResponse> GetWordDefinitionAsync(string word)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_dictionaryApiUrl}{word}");
                if(!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error getting word: {response.StatusCode}");
                    return null;
                }
                var content = await response.Content.ReadAsStringAsync();
                var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponse>>(content);
                return dictionaryResponse?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }
    }
}
