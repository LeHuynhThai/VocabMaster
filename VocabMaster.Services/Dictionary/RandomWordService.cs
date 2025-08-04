using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.Services.Dictionary
{
    public class RandomWordService : IRandomWordService
    {
        private readonly ILogger<RandomWordService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly IDictionaryLookupService _dictionaryLookupService;
        private readonly Random _random = new Random();
        private const int MAX_ATTEMPTS = 5;

        public RandomWordService(
            ILogger<RandomWordService> logger,
            IVocabularyRepo vocabularyRepository,
            ILearnedWordRepo learnedWordRepository,
            IDictionaryLookupService dictionaryLookupService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _dictionaryLookupService = dictionaryLookupService ?? throw new ArgumentNullException(nameof(dictionaryLookupService));
        }

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

                // try to get word definition from database
                try 
                {
                    _logger.LogInformation("Getting definition for word: {Word}", vocabulary.Word);
                    var response = await _dictionaryLookupService.GetWordDefinitionFromDatabase(vocabulary.Word);
                    
                    if (response == null)
                    {
                        _logger.LogWarning("GetWordDefinitionFromDatabase returned null for word: {Word}, trying from direct API...", vocabulary.Word);
                        response = await _dictionaryLookupService.GetWordDefinition(vocabulary.Word);
                    }
                    
                    if (response == null)
                    {
                        _logger.LogWarning("Both database and direct API returned null for word: {Word}. Creating basic response...", vocabulary.Word);
                        return null;
                    }
                    
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting word definition for {Word}", vocabulary.Word);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random word");
                return null;
            }
        }

        public async Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId)
        {
            try
            {
                _logger.LogInformation("Getting random word excluding learned ones for user {UserId}", userId);
                // First try to get a word excluding learned ones
                var result = await TryGetRandomWordExcludeLearned(userId);

                // If no unlearned word is found or there's an issue, fall back to any random word
                if (result == null)
                {
                    _logger.LogInformation("Falling back to any random word for user {UserId}", userId);
                    return await GetRandomWord();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRandomWordExcludeLearned for user {UserId}, falling back to any random word", userId);
                // In case of any error, still try to return a random word
                return await GetRandomWord();
            }
        }

        private async Task<DictionaryResponseDto> TryGetRandomWordExcludeLearned(int userId)
        {
            try
            {
                _logger.LogInformation("Getting learned words for user {UserId}", userId);
                var learnedVocabularies = await _learnedWordRepository.GetByUserId(userId);

                if (learnedVocabularies == null)
                {
                    _logger.LogWarning("learnedVocabularies is null for user {UserId}", userId);
                    learnedVocabularies = new List<Core.Entities.LearnedWord>();
                }
                
                if (!learnedVocabularies.Any())
                {
                    _logger.LogInformation("No learned vocabularies found for user {UserId}, returning any random word", userId);
                    return await GetRandomWord();
                }

                var learnedWords = learnedVocabularies.Select(lv => lv.Word.ToLowerInvariant()).ToHashSet();
                _logger.LogInformation("User {UserId} has learned {Count} words", userId, learnedWords.Count);

                // Try to get an unlearned word
                var vocabulary = await _vocabularyRepository.GetRandomExcludeLearned(learnedWords.ToList());

                if (vocabulary == null)
                {
                    _logger.LogInformation("No unlearned words found for user {UserId} in the first attempt", userId);

                    // If all words from the repository are learned, try multiple random words
                    // until we find one that's not in the learned list
                    for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
                    {
                        var randomVocab = await _vocabularyRepository.GetRandom();
                        if (randomVocab != null && !learnedWords.Contains(randomVocab.Word.ToLowerInvariant()))
                        {
                            _logger.LogInformation("Found unlearned random word on attempt {Attempt}: {Word}", attempt + 1, randomVocab.Word);
                            try
                            {
                                _logger.LogInformation("Getting definition for word: {Word}", randomVocab.Word);
                                var response = await _dictionaryLookupService.GetWordDefinitionFromDatabase(randomVocab.Word);
                                
                                if (response == null)
                                {
                                    _logger.LogWarning("GetWordDefinitionFromDatabase returned null for word: {Word}, trying direct API...", randomVocab.Word);
                                    response = await _dictionaryLookupService.GetWordDefinition(randomVocab.Word);
                                }
                                
                                if (response != null)
                                {
                                    return response;
                                }
                                else
                                {
                                    _logger.LogWarning("Both database and API failed for word: {Word}, creating basic response", randomVocab.Word);
                                    // Create a minimal response if both database and API fail
                                    return new DictionaryResponseDto
                                    {
                                        Word = randomVocab.Word,
                                        Vietnamese = randomVocab.Vietnamese
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error getting definition for word {Word}, creating basic response", randomVocab.Word);
                                // Return basic info even if lookup fails
                                return new DictionaryResponseDto
                                {
                                    Word = randomVocab.Word,
                                    Vietnamese = randomVocab.Vietnamese
                                };
                            }
                        }
                    }

                    _logger.LogInformation("All attempts to find unlearned word failed, returning any random word");
                    return await GetRandomWord();
                }

                _logger.LogInformation("Found random unlearned word: {Word}", vocabulary.Word);
                try
                {
                    _logger.LogInformation("Getting definition for word: {Word}", vocabulary.Word);
                    var response = await _dictionaryLookupService.GetWordDefinitionFromDatabase(vocabulary.Word);
                    
                    if (response == null)
                    {
                        _logger.LogWarning("GetWordDefinitionFromDatabase returned null for word: {Word}, trying direct API...", vocabulary.Word);
                        response = await _dictionaryLookupService.GetWordDefinition(vocabulary.Word);
                    }
                    
                    if (response == null)
                    {
                        _logger.LogWarning("Both database and API failed for word: {Word}, creating basic response", vocabulary.Word);
                        // Create a minimal response if both database and API fail
                        return new DictionaryResponseDto 
                        {
                            Word = vocabulary.Word,
                            Vietnamese = vocabulary.Vietnamese
                        };
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting definition for word {Word}, creating basic response", vocabulary.Word);
                    // Return basic info even if lookup fails
                    return new DictionaryResponseDto 
                    {
                        Word = vocabulary.Word,
                        Vietnamese = vocabulary.Vietnamese
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TryGetRandomWordExcludeLearned for user {UserId}", userId);
                return null;
            }
        }
    }
}