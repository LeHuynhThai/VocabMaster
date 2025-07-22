using System.Text.Json;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Services
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly string _dictionaryApiUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";

        public DictionaryService(
            ILogger<DictionaryService> logger,
            IVocabularyRepository vocabularyRepository)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _vocabularyRepository = vocabularyRepository;
        }

        // get random word and its definition
        public async Task<DictionaryResponseDto> GetRandomWordAsync()
        {
            try
            {
                var vocabulary = await _vocabularyRepository.GetRandomAsync();
                if (vocabulary == null)
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
        public async Task<DictionaryResponseDto> GetWordDefinitionAsync(string word)
        {
            try
            {
                // create response by get word definition from dictionary api
                var response = await _httpClient.GetAsync($"{_dictionaryApiUrl}{word}");
                if (!response.IsSuccessStatusCode) // if not success, return null
                {
                    _logger.LogError($"Error getting word definition: {response.StatusCode}");
                    return null;
                }
                // get content from response
                var content = await response.Content.ReadAsStringAsync();
                // deserialize content to dictionary response
                var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponseDto>>(content);
                // return first dictionary response
                return dictionaryResponse?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting word definition", ex);
                return null;
            }
        }
    }
}
