using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Services.Interfaces;
using System.Security.Claims;

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

        [HttpPost]
        public async Task<IActionResult> GenerateWord()
        {
            try
            {
                var randomWord = await _dictionaryService.GetRandomWordAsync();

                if (randomWord == null)
                {
                    _logger.LogWarning("No word found");
                    ModelState.AddModelError("", "Cannot generate random word. Please try again.");
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
                return BadRequest("Word cannot be empty");
            }

            try
            {
                var definition = await _dictionaryService.GetWordDefinitionAsync(word);

                if (definition == null)
                {
                    _logger.LogWarning($"No definition found for word: {word}");
                    ModelState.AddModelError("", $"No definition found for word: {word}");
                    return View("Index");
                }

                ViewBag.RandomWord = definition;
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting word definition: {word}");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsLearned(string word)
        {
            _logger.LogInformation("MarkAsLearned called with word: {Word}", word);

            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word is empty");
                return Json(new { success = false, message = "Từ vựng không được để trống" });
            }

            // Lấy UserId từ Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                _logger.LogWarning("Không tìm thấy UserId trong Claims");
                return Json(new { success = false, message = "Vui lòng đăng nhập lại" });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("UserId không hợp lệ: {UserId}", userIdClaim.Value);
                return Json(new { success = false, message = "Thông tin người dùng không hợp lệ" });
            }

            try
            {
                var result = await _vocabularyService.MarkWordAsLearnedAsync(userId, word.Trim());
                if (result.Success)
                {
                    _logger.LogInformation("User {UserId} marked word '{Word}' as learned", userId, word);
                    return Json(new { success = true, message = $"Đã đánh dấu từ '{word}' là đã học" });
                }
                else
                {
                    _logger.LogWarning("User {UserId} failed to mark word '{Word}' as learned: {Reason}", userId, word, result.ErrorMessage);
                    return Json(new { success = false, message = result.ErrorMessage ?? "Không thể đánh dấu từ đã học. Vui lòng thử lại." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu từ '{Word}' là đã học", word);
                return Json(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLearnedWords()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("Cannot get UserId from Claims");
                    return BadRequest("Cannot determine user");
                }

                var learnedWords = await _vocabularyService.GetUserLearnedVocabulariesAsync(userId);
                return Json(new { success = true, words = learnedWords.Select(lv => lv.Word) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learned words");
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }
        
    }
}
