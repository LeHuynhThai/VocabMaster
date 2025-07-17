using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Models;
using VocabMaster.Repositories;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations
{
    public class VocabularyService : IVocabularyService
    {
        private readonly ILearnedVocabularyRepository _learnedVocabularyRepository;

        public VocabularyService(ILearnedVocabularyRepository learnedVocabularyRepository)
        {
            _learnedVocabularyRepository = learnedVocabularyRepository;
        }

        public async Task<bool> MarkWordAsLearnedAsync(string userId, string word, string note = null)
        {
            if (await IsWordLearnedAsync(userId, word))
            {
                return false; // Từ đã được học rồi
            }

            var learnedVocabulary = new LearnedVocabulary
            {
                UserId = userId,
                Word = word,
                Note = note,
                LearnedDate = DateTime.UtcNow
            };

            return await _learnedVocabularyRepository.AddLearnedWordAsync(learnedVocabulary);
        }

        public async Task<string> GetRandomUnlearnedWordAsync(string userId, List<string> allWords)
        {
            var learnedWords = await _learnedVocabularyRepository.GetLearnedWordsAsync(userId);
            var unlearnedWords = allWords.Except(learnedWords).ToList();

            if (!unlearnedWords.Any())
            {
                return null; // Đã học hết tất cả các từ
            }

            var random = new Random();
            var randomIndex = random.Next(0, unlearnedWords.Count);
            return unlearnedWords[randomIndex];
        }

        public async Task<List<LearnedVocabulary>> GetUserLearnedVocabulariesAsync(string userId)
        {
            return await _learnedVocabularyRepository.GetUserLearnedVocabulariesAsync(userId);
        }

        public async Task<bool> IsWordLearnedAsync(string userId, string word)
        {
            return await _learnedVocabularyRepository.IsWordLearnedAsync(userId, word);
        }

        public async Task<int> GetUserProgressAsync(string userId)
        {
            return await _learnedVocabularyRepository.GetTotalLearnedWordsAsync(userId);
        }
    }
}


