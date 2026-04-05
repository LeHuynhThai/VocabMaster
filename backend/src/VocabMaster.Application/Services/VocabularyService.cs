using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Exceptions;
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
            // 1. take learned words from user
            var learnedWords = await _vocab.GetLearnedWords(userId);
            var learnedWordsList = learnedWords.Select(lw => lw.Word).ToList();

            // 2. get random word from vocabulary exclude learned words
            var randomWord = await _vocab.GetRandomExcludeLearned(learnedWordsList);

            // 3. if user learned all words, throw exception
            if (randomWord == null)
            {
                throw new DomainException("Bạn đã học hết tất cả từ vựng trong hệ thống!");
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

        public async Task<Vocabulary?> GetVocabularyByWord(string word)
        {
            return await _vocab.GetVocabularyByWord(word);
        }
    }
}
