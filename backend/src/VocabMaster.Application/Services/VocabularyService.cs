using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;

namespace VocabMaster.Application.Services
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IVocabularyRepo _vocab;

        public VocabularyService(IVocabularyRepo vocabulary)
        {
            _vocab = vocabulary;
        }

        public async Task<Vocabulary?> GetRandomWord(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            // 1. take learned words from user
            var learnedWords = await _vocab.GetLearnedWords(userId);
            var learnedWordsList = learnedWords.Select(lw => lw.Word).ToList();

            // 2. get random word from vocabulary exclude learned words
            return await _vocab.GetRandomExcludeLearned(learnedWordsList);
        }

        // Get learned words
        public async Task<List<LearnedWord>> GetLearnedWords(int userId)
        {
            return await _vocab.GetLearnedWords(userId);
        }

        // Add learned word
        public async Task<LearnedWord> AddLearnedWord(string word, int userId)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException("Word cannot be null or empty", nameof(word));
            }

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "UserId must be greater than zero.");
            }

            var learnedWord = new LearnedWord
            {
                Word = word.Trim(),
                UserId = userId,
                LearnedAt = DateTime.UtcNow
            };

            return await _vocab.AddLearnedWord(learnedWord);
        }

        // Remove learned word
        public async Task<bool> RemoveLearnedWord(int learnedWordId, int userId)
        {
            if (learnedWordId <= 0 || userId <= 0)
            {
                return false;
            }

            return await _vocab.RemoveLearnedWord(learnedWordId, userId);
        }

        public async Task<Vocabulary?> GetVocabularyByWord(string word)
        {
            return await _vocab.GetVocabularyByWord(word);
        }
    }
}
