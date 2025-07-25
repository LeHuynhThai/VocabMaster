using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LearnedWordController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;
        private readonly ILogger<LearnedWordController> _logger;

        public LearnedWordController(IVocabularyService vocabularyService, ILogger<LearnedWordController> logger)
        {
            _vocabularyService = vocabularyService;
            _logger = logger;
        }

        // API endpoint to get user's learned words
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetLearnedWords()
        {
            try
            {
                // Get UserId from Claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogError("UserId not found in Claims");
                    return Unauthorized();
                }

                // Get learned words
                var learnedWords = await _vocabularyService.GetUserLearnedVocabularies(userId);
                return Ok(learnedWords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading learned words");
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }

        // API endpoint to add a word to learned words
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> AddLearnedWord([FromBody] AddLearnedWordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Word))
            {
                return BadRequest(new { message = "Word cannot be empty" });
            }

            // Get UserId from Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("UserId not found in Claims");
                return Unauthorized();
            }

            try
            {
                var result = await _vocabularyService.MarkWordAsLearned(userId, request.Word.Trim());
                if (result.Success)
                {
                    return Ok(new { 
                        id = result.Data?.Id ?? 0, 
                        word = request.Word.Trim(),
                        userId = userId 
                    });
                }
                else
                {
                    return BadRequest(new { message = result.ErrorMessage ?? "Cannot mark word as learned. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking word as learned: {Word}", request.Word);
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }

        // API endpoint to remove a learned word
        [HttpDelete("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteLearnedWord(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid word ID" });
            }

            // Get UserId from Claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogError("UserId not found in Claims");
                return Unauthorized();
            }

            try
            {
                var result = await _vocabularyService.RemoveLearnedWordById(userId, id);
                if (result)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return NotFound(new { message = "Word not found or you don't have permission to remove it." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing learned word: {Id}", id);
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }
    }

    public class AddLearnedWordRequest
    {
        public string Word { get; set; }
    }
}
