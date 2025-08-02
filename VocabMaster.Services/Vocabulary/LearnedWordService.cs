using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.Services.Vocabulary
{
    public class LearnedWordService : ILearnedWordService
    {
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly IWordStatusService _wordStatusService;
        private readonly ILogger<LearnedWordService> _logger;

        public LearnedWordService(
            ILearnedWordRepo learnedWordRepository,
            IWordStatusService wordStatusService,
            ILogger<LearnedWordService> logger)
        {
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _wordStatusService = wordStatusService ?? throw new ArgumentNullException(nameof(wordStatusService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MarkWordResultDto> MarkWordAsLearned(int userId, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return new MarkWordResultDto { Success = false, ErrorMessage = "Word cannot be empty" };
            }

            try
            {
                // Check if the word is already learned
                if (await _wordStatusService.IsWordLearned(userId, word))
                {
                    _logger.LogWarning("User {UserId} tried to mark already learned word: {Word}", userId, word);
                    return new MarkWordResultDto { Success = false, ErrorMessage = "This word is already marked as learned" };
                }

                // Create new learned word
                var learnedWord = new LearnedWord
                {
                    UserId = userId,
                    Word = word,
                };

                var result = await _learnedWordRepository.Add(learnedWord);

                // Invalidate cache
                _wordStatusService.InvalidateUserCache(userId);

                if (result)
                {
                    _logger.LogInformation("Successfully marked word '{Word}' as learned for user {UserId}", word, userId);
                    return new MarkWordResultDto { Success = true, Data = learnedWord };
                }
                else
                {
                    _logger.LogWarning("Failed to mark word as learned for user {UserId}: {Word}", userId, word);
                    return new MarkWordResultDto { Success = false, ErrorMessage = "Failed to save learned word. Please try again." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word as learned for user {UserId}: {Word}", userId, word);
                return new MarkWordResultDto { Success = false, ErrorMessage = "An error occurred. Please try again." };
            }
        }

        public async Task<List<LearnedWord>> GetUserLearnedVocabularies(int userId)
        {
            try
            {
                _logger.LogInformation("Getting learned words for user {UserId}", userId);
                return await _learnedWordRepository.GetByUserId(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned words for user {UserId}", userId);
                return new List<LearnedWord>();
            }
        }

        public async Task<bool> RemoveLearnedWordById(int userId, int wordId)
        {
            try
            {
                _logger.LogInformation("Removing learned word with ID {WordId} for user {UserId}", wordId, userId);

                // Verify the word belongs to the user
                var learnedWord = await _learnedWordRepository.GetById(wordId);
                if (learnedWord == null || learnedWord.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} tried to remove learned word with ID {WordId} that doesn't exist or doesn't belong to them", userId, wordId);
                    return false;
                }

                var result = await _learnedWordRepository.Delete(wordId);

                // Invalidate cache
                _wordStatusService.InvalidateUserCache(userId);

                _logger.LogInformation("Successfully removed learned word with ID {WordId} for user {UserId}", wordId, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing learned word by ID {WordId} for user {UserId}", wordId, userId);
                return false;
            }
        }

        public async Task<LearnedWord> GetLearnedWordById(int userId, int wordId)
        {
            try
            {
                _logger.LogInformation("Getting learned word with ID {WordId} for user {UserId}", wordId, userId);

                var learnedWord = await _learnedWordRepository.GetById(wordId);

                // Check if the word exists and belongs to the user
                if (learnedWord == null || learnedWord.UserId != userId)
                {
                    _logger.LogWarning("Learned word with ID {WordId} not found or doesn't belong to user {UserId}", wordId, userId);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved learned word with ID {WordId} for user {UserId}", wordId, userId);
                return learnedWord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned word with ID {WordId} for user {UserId}", wordId, userId);
                return null;
            }
        }
    }
} 