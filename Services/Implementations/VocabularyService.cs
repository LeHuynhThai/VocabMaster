using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VocabMaster.Entities;
using VocabMaster.Repositories.Interfaces;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations
{
    public class VocabularyService : IVocabularyService
    {
        private readonly ILearnedVocabularyRepository _learnedVocabularyRepository;
        private readonly ILogger<VocabularyService> _logger; // logger for logging errors

        public VocabularyService(
            ILearnedVocabularyRepository learnedVocabularyRepository,
            ILogger<VocabularyService> logger)
        {
            _learnedVocabularyRepository = learnedVocabularyRepository;
            _logger = logger;
        }

        public class MarkWordResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }

        public async Task<MarkWordResult> MarkWordAsLearnedAsync(int userId, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new MarkWordResult { Success = false, ErrorMessage = "Từ vựng không được để trống" };

            try
            {
                // Kiểm tra đã học chưa
                var learnedWords = await _learnedVocabularyRepository.GetByUserIdAsync(userId);
                if (learnedWords.Any(lv => lv.Word.Equals(word, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("User {UserId} tried to mark already learned word: {Word}", userId, word);
                    return new MarkWordResult { Success = false, ErrorMessage = "Từ này đã được đánh dấu là đã học" };
                }

                var learnedVocabulary = new LearnedVocabulary
                {
                    UserId = userId,
                    Word = word
                };

                var addResult = await _learnedVocabularyRepository.AddAsync(learnedVocabulary);
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
        public async Task<List<LearnedVocabulary>> GetUserLearnedVocabulariesAsync(int userId)
        {
            try
            {
                return await _learnedVocabularyRepository.GetByUserIdAsync(userId); // GetByUserIdAsync method in LearnedVocabularyRepository
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned vocabularies for user {UserId}", userId);
                return new List<LearnedVocabulary>();
            }
        }
        
        // Remove a learned word for a specific user
        public async Task<bool> RemoveLearnedWordAsync(int userId, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;
                
            try
            {
                // Kiểm tra từ có tồn tại không
                var learnedWords = await _learnedVocabularyRepository.GetByUserIdAsync(userId);
                var learnedWord = learnedWords.FirstOrDefault(lv => 
                    lv.UserId == userId && 
                    lv.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
                
                if (learnedWord == null)
                {
                    _logger.LogWarning("User {UserId} tried to remove non-existent learned word: {Word}", userId, word);
                    return false;
                }
                
                // Xóa từ đã học
                return await _learnedVocabularyRepository.DeleteAsync(learnedWord.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing learned word for user {UserId}: {Word}", userId, word);
                return false;
            }
        }
    }
}


