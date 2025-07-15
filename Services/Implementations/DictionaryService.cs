using System.Text.Json;
using VocabMaster.Models;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        private readonly string _apiUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";
        private readonly Random _random = new Random();
        private readonly string[] _commonWords = new [] 
        {
            "hello", "world", "love", "happy", "sad", "book", "computer", "friend",
            "family", "work", "play", "eat", "sleep", "dream", "hope", "peace",
            "joy", "smile", "laugh", "learn", "teach", "create", "build", "grow",
            "run", "walk", "talk", "think", "feel", "believe", "understand", "remember",
            "imagine", "create", "build", "grow", "run", "walk", "talk", "think",
            "feel", "believe", "understand", "remember", "imagine", "create", "build", "grow",
            "run", "walk", "talk", "think", "feel", "believe", "understand", "remember",
            "imagine", "create", "build", "grow", "run", "walk", "talk", "think",
            "feel", "believe", "understand", "remember", "imagine", "create", "build", "grow",
        };
        
        public DictionaryService(ILogger<DictionaryService> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<DictionaryResponse> GetRandomWordAsync()
        {
            try
            {
                var random = _commonWords[_random.Next(_commonWords.Length)];
                var word = await _httpClient.GetAsync($"{_apiUrl}{random}");
                if(!word.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error getting word: {word.StatusCode}");
                    return null;
                }
                var content = await word.Content.ReadAsStringAsync();
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
