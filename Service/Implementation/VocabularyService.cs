using Repository.Implementation;
using Repository.Interfaces;
using Service.Interfaces;
using Repository.Entities;

namespace Service.Implementation
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
            // 1. take learned words from user
            var learnedWords = await _vocab.GetLearnedWords(userId);
            var learnedWordsList = learnedWords.Select(lw => lw.Word).ToList();

            // 2. get random word from vocabulary exclude learned words
            var randomWord = await _vocab.GetRandomExcludeLearned(learnedWordsList);

            // 3. if user learned all words, throw exception
            if (randomWord == null)
            {
                throw new InvalidOperationException("Bạn đã học hết tất cả từ vựng trong hệ thống!");
            }

            return randomWord;
        }

        // Get learned words
        public async Task<List<LearnedWord>> GetLearnedWords(int userId)
        {
            return await _vocab.GetLearnedWords(userId);
        }

        // Add learned word
        public async Task<LearnedWord> AddLearnedWord(LearnedWord learnedWord)
        {
            return await _vocab.AddLearnedWord(learnedWord);
        }

        // Remove learned word
        public async Task<bool> RemoveLearnedWord(int learnedWordId)
        {
            return await _vocab.RemoveLearnedWord(learnedWordId);
        }
    }
}
