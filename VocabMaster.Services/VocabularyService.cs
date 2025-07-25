using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services
{
    public class VocabularyService : IVocabularyService
    {
        private readonly ILearnedWordRepo _learnedVocabularyRepository;
        private readonly ILogger<VocabularyService> _logger; // logger for logging errors

        public VocabularyService(
            ILearnedWordRepo learnedVocabularyRepository,
            ILogger<VocabularyService> logger)
        {
            _learnedVocabularyRepository = learnedVocabularyRepository;
            _logger = logger;
        }

        public async Task<MarkWordResult> MarkWordAsLearned(int userId, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new MarkWordResult { Success = false, ErrorMessage = "Từ vựng không được để trống" };

            try
            {
                var learnedWords = await _learnedVocabularyRepository.GetByUserId(userId); // GetByUserIdAsync method in LearnedWordRepo
                if (learnedWords.Any(lv => lv.Word.Equals(word, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("User {UserId} tried to mark already learned word: {Word}", userId, word);
                    return new MarkWordResult { Success = false, ErrorMessage = "Từ này đã được đánh dấu là đã học" };
                }

                var learnedVocabulary = new LearnedWord
                {
                    UserId = userId,
                    Word = word
                };

                var addResult = await _learnedVocabularyRepository.Add(learnedVocabulary); // Add method in LearnedWordRepo
                if (addResult)
                {
                    return new MarkWordResult { Success = true };
                }
                else
                {
                    _logger.LogWarning("Failed to add learned word for user {UserId}: {Word}", userId, word);
                    return new MarkWordResult { Success = false, ErrorMessage = "Không thể lưu từ đã học. Vui lòng thử lại." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word as learned for user {UserId}: {Word}", userId, word);
                return new MarkWordResult { Success = false, ErrorMessage = "Đã xảy ra lỗi. Vui lòng thử lại." };
            }
        }

        // get all learned vocabularies for a specific user
        public async Task<List<LearnedWord>> GetUserLearnedVocabularies(int userId)
        {
            try
            {
                return await _learnedVocabularyRepository.GetByUserId(userId); // GetByUserId method in LearnedWordRepo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned vocabularies for user {UserId}", userId);
                return new List<LearnedWord>();
            }
        }
        
        // Remove a learned word for a specific user
        public async Task<bool> RemoveLearnedWord(int userId, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;
                
            try
            {
                var learnedWords = await _learnedVocabularyRepository.GetByUserId(userId); // GetByUserId method in LearnedWordRepo
                var learnedWord = learnedWords.FirstOrDefault(lv => 
                    lv.UserId == userId && 
                    lv.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
                
                if (learnedWord == null)
                {
                    _logger.LogWarning("User {UserId} tried to remove non-existent learned word: {Word}", userId, word);
                    return false;
                }
                
                return await _learnedVocabularyRepository.Delete(learnedWord.Id); // Delete method in LearnedWordRepo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing learned word for user {UserId}: {Word}", userId, word);
                return false;
            }
        }

        // Remove a learned word by ID
        public async Task<bool> RemoveLearnedWordById(int userId, int wordId)
        {
            try
            {
                var learnedWord = await _learnedVocabularyRepository.GetById(wordId);
                if (learnedWord == null || learnedWord.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} tried to remove learned word with ID {WordId} that doesn't exist or doesn't belong to them", userId, wordId);
                    return false;
                }
                
                return await _learnedVocabularyRepository.Delete(wordId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing learned word by ID: {WordId} for user {UserId}", wordId, userId);
                return false;
            }
        }
    }
}


