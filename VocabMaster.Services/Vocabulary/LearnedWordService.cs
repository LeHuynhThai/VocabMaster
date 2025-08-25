using AutoMapper;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Vocabulary;

namespace VocabMaster.Services.Vocabulary
{
    // Service xử lý logic từ đã học của user (thêm, xóa, lấy danh sách, phân trang...)
    public class LearnedWordService : ILearnedWordService
    {
        private readonly ILearnedWordRepo _learnedWordRepository;
        private readonly IWordStatusService _wordStatusService;
        private readonly IMapper _mapper;
        private readonly ILogger<LearnedWordService> _logger;

        // Hàm khởi tạo service, inject các dependency cần thiết
        public LearnedWordService(
            ILearnedWordRepo learnedWordRepository,
            IWordStatusService wordStatusService,
            IMapper mapper,
            ILogger<LearnedWordService> logger)
        {
            _learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
            _wordStatusService = wordStatusService ?? throw new ArgumentNullException(nameof(wordStatusService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Đánh dấu một từ là đã học cho user
        public async Task<MarkWordResultDto> MarkWordAsLearned(int userId, string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return new MarkWordResultDto { Success = false, ErrorMessage = "Word cannot be empty" };
            }

            try
            {
                // Kiểm tra từ đã học chưa
                if (await _wordStatusService.IsWordLearned(userId, word))
                {
                    _logger.LogWarning("User {UserId} tried to mark already learned word: {Word}", userId, word);
                    return new MarkWordResultDto { Success = false, ErrorMessage = "This word is already marked as learned" };
                }

                // Tạo bản ghi từ đã học mới
                var learnedWord = new LearnedWord
                {
                    UserId = userId,
                    Word = word,
                };

                var result = await _learnedWordRepository.Add(learnedWord);

                // Xóa cache
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

        // Lấy danh sách từ đã học của user
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

        // Xóa một từ đã học theo id (chỉ cho phép xóa từ của chính user đó)
        public async Task<bool> RemoveLearnedWordById(int userId, int wordId)
        {
            try
            {
                _logger.LogInformation("Removing learned word with ID {WordId} for user {UserId}", wordId, userId);

                // Kiểm tra từ có thuộc về user không
                var learnedWord = await _learnedWordRepository.GetById(wordId);
                if (learnedWord == null || learnedWord.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} tried to remove learned word with ID {WordId} that doesn't exist or doesn't belong to them", userId, wordId);
                    return false;
                }

                var result = await _learnedWordRepository.Delete(wordId);

                // Xóa cache
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

        // Lấy chi tiết một từ đã học theo id (chỉ trả về nếu thuộc về user)
        public async Task<LearnedWord> GetLearnedWordById(int userId, int wordId)
        {
            try
            {
                _logger.LogInformation("Getting learned word with ID {WordId} for user {UserId}", wordId, userId);

                var learnedWord = await _learnedWordRepository.GetById(wordId);

                // Kiểm tra từ có tồn tại và thuộc về user không
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

        // Lấy danh sách từ đã học có phân trang cho user
        public async Task<(List<LearnedWordDto> Items, int TotalCount, int TotalPages)> GetPaginatedLearnedWords(int userId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting paginated learned words for user {UserId}, page {PageNumber}, size {PageSize}", 
                    userId, pageNumber, pageSize);
                
                var (items, totalCount) = await _learnedWordRepository.GetPaginatedByUserId(userId, pageNumber, pageSize);
                var dtos = _mapper.Map<List<LearnedWordDto>>(items);
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                
                _logger.LogInformation("Retrieved {Count} learned words for user {UserId}, total {Total} items, {Pages} pages", 
                    items.Count, userId, totalCount, totalPages);
                
                return (dtos, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated learned words for user {UserId}", userId);
                throw;
            }
        }
    }
}