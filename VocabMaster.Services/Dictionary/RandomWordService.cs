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

                _logger.LogInformation("Found random word: {Word}", vocabulary.Word);
                return await _dictionaryLookupService.GetWordDefinitionFromCache(vocabulary.Word);
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

                if (learnedVocabularies == null || !learnedVocabularies.Any())
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
                            return await _dictionaryLookupService.GetWordDefinitionFromCache(randomVocab.Word);
                        }
                    }

                    _logger.LogInformation("All attempts to find unlearned word failed, returning any random word");
                    return await GetRandomWord();
                }

                _logger.LogInformation("Found random unlearned word: {Word}", vocabulary.Word);
                return await _dictionaryLookupService.GetWordDefinitionFromCache(vocabulary.Word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TryGetRandomWordExcludeLearned for user {UserId}", userId);
                return null;
            }
        }
    }
}