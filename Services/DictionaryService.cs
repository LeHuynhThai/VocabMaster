using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Services
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly ILearnedWordRepo _learnedVocabularyRepository;
        private readonly string _dictionaryApiUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";

        public DictionaryService(
            ILogger<DictionaryService> logger,
            IVocabularyRepo vocabularyRepository,
            ILearnedWordRepo learnedVocabularyRepository)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _vocabularyRepository = vocabularyRepository;
            _learnedVocabularyRepository = learnedVocabularyRepository;
        }

        // get random word and its definition
        public async Task<DictionaryResponseDto> GetRandomWord()
        {
            try
            {
                var vocabulary = await _vocabularyRepository.GetRandom();
                if (vocabulary == null)
                {
                    _logger.LogWarning("No vocabulary found");
                    return null;
                }
                return await GetWordDefinition(vocabulary.Word); // return random word and definition
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }
        
        // get random word excluding learned words
        public async Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId)
        {
            try
            {
                // Get user's learned words
                var learnedVocabularies = await _learnedVocabularyRepository.GetByUserId(userId);
                var learnedWords = learnedVocabularies.Select(lv => lv.Word).ToList();
                
                // Get random word excluding learned words
                var vocabulary = await _vocabularyRepository.GetRandomExcludeLearned(learnedWords);
                if (vocabulary == null)
                {
                    _logger.LogWarning("No vocabulary found or all words have been learned");
                    return null;
                }
                
                // Get definition for the random word
                return await GetWordDefinition(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word excluding learned words");
                return null;
            }
        }

        // get word definition from dictionary api
        public async Task<DictionaryResponseDto> GetWordDefinition(string word)
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
