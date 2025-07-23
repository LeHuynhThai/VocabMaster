using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers
{
    [Authorize]
    public class LearnedWordController : Controller
    {
        private readonly IVocabularyService _vocabularyService;
        private readonly ILogger<LearnedWordController> _logger;

        public LearnedWordController(IVocabularyService vocabularyService, ILogger<LearnedWordController> logger)
        {
            _vocabularyService = vocabularyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get UserId from Claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogError("UserId not found in Claims");
                    TempData["Error"] = "Please login again";
                    return RedirectToAction("Index", "Home");
                }

                // Get learned words
                var learnedWords = await _vocabularyService.GetUserLearnedVocabularies(userId);
                return View(learnedWords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading learned words");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLearnedWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                TempData["Error"] = "Từ vựng không được để trống";
                return RedirectToAction("Index");
            }

            // Get UserId from Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("UserId không hợp lệ hoặc không tìm thấy");
                TempData["Error"] = "Vui lòng đăng nhập lại";
                return RedirectToAction("Index");
            }

            try
            {
                var result = await _vocabularyService.RemoveLearnedWord(userId, word.Trim());
                if (result)
                {
                    TempData["Success"] = $"Đã xóa từ '{word}' khỏi danh sách từ đã học";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa từ. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa từ đã học: {Word}", word);
                TempData["Error"] = "Đã xảy ra lỗi. Vui lòng thử lại.";
            }

            return RedirectToAction("Index");
        }
    }
}
