using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Services.Interfaces;
using System.Security.Claims;
using VocabMaster.Core.Entities;

namespace VocabMaster.Controllers
{
    [Authorize]
    public class WordGeneratorController : Controller
    {
        private readonly IDictionaryService _dictionaryService; // service for random word
        private readonly IVocabularyService _vocabularyService; // service for learned vocabulary
        private readonly ILogger<WordGeneratorController> _logger;

        public WordGeneratorController(
            IDictionaryService dictionaryService,
            IVocabularyService vocabularyService,
            ILogger<WordGeneratorController> logger)
        {
            _dictionaryService = dictionaryService;
            _vocabularyService = vocabularyService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LearnedWords()
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
                var learnedWords = await _vocabularyService.GetUserLearnedVocabulariesAsync(userId);
                return View(learnedWords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading learned words");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateWord()
        {
            try
            {
                var randomWord = await _dictionaryService.GetRandomWordAsync();

                if (randomWord == null)
                {
                    _logger.LogError("No word found");
                    ModelState.AddModelError("", "No word found");
                    return View("Index");
                }       

                ViewBag.RandomWord = randomWord;
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating random word");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetWordDefinition(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                TempData["Error"] = "Word cannot be empty";
                return View("Index");
            }

            try
            {
                var definition = await _dictionaryService.GetWordDefinitionAsync(word);

                if (definition == null)
                {
                    TempData["Error"] = $"No definition found for word: {word}";
                    return View("Index");
                }

                ViewBag.RandomWord = definition;
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting word definition: {Word}", word);
                TempData["Error"] = "An error occurred. Please try again.";
                return View("Index");
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsLearned(string word)
        {
            // Check if word is empty
            if (string.IsNullOrWhiteSpace(word))
            {
                TempData["Error"] = "Word cannot be empty";
                return RedirectToAction("Index");
            }

            // Get UserId from Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                _logger.LogError("UserId not found in Claims");
                TempData["Error"] = "Please login again";
                return RedirectToAction("Index");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("UserId không hợp lệ: {UserId}", userIdClaim.Value);
                TempData["Error"] = "Invalid user information";
                return RedirectToAction("Index");
            }

            try
            {
                var result = await _vocabularyService.MarkWordAsLearnedAsync(userId, word.Trim());
                if (result.Success)
                {
                    TempData["Success"] = $"Word '{word}' has been marked as learned";
                }
                else
                {
                    _logger.LogError("Cannot mark word as learned: {Word}", word);
                    TempData["Error"] = result.ErrorMessage ?? "Cannot mark word as learned. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error marking word as learned: {Word}", word);
                TempData["Error"] = "An error occurred. Please try again.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLearnedWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                TempData["Error"] = "Từ vựng không được để trống";
                return RedirectToAction("LearnedWords");
            }

            // Get UserId from Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("UserId không hợp lệ hoặc không tìm thấy");
                TempData["Error"] = "Vui lòng đăng nhập lại";
                return RedirectToAction("LearnedWords");
            }

            try
            {
                var result = await _vocabularyService.RemoveLearnedWordAsync(userId, word.Trim());
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

            return RedirectToAction("LearnedWords");
        }
    }
}
