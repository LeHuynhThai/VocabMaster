using System.Text.Json;
using VocabMaster.Models;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        private readonly IVocabularyService _vocabularyService;
        private readonly string _dictionaryApiUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";


        public DictionaryService(ILogger<DictionaryService> logger, IVocabularyService vocabularyService)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _vocabularyService = vocabularyService;
        }

        // get random word from vocabulary service and get word definition from dictionary api
        public async Task<DictionaryResponse> GetRandomWordAsync()
        {
            try
            {
                var vocabulary = await _vocabularyService.GetRandomVocabularyAsync(); // get random word from vocabulary service
                if (vocabulary == null) // if no vocabulary, return null
                {
                    _logger.LogWarning("No vocabulary found");
                    return null;
                }
                return await GetWordDefinitionAsync(vocabulary.Word); // return random word and definition
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }
        // get word definition from dictionary api
        public async Task<DictionaryResponse> GetWordDefinitionAsync(string word)
        {
            try
            {
                // create response by get word definition from dictionary api
                var response = await _httpClient.GetAsync($"{_dictionaryApiUrl}{word}");
                if (!response.IsSuccessStatusCode) // if not success, return null
                {
                    _logger.LogError($"Error getting word: {response.StatusCode}");
                    return null;
                }
                // get content from response
                var content = await response.Content.ReadAsStringAsync();
                // deserialize content to dictionary response
                var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponse>>(content);
                // return first dictionary response
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
