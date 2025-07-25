using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Entities;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WordGeneratorController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly IVocabularyService _vocabularyService;
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

        // API endpoint for getting a random word
        [HttpGet("getrandomword")]
        [Produces("application/json")]
        public async Task<IActionResult> GetRandomWord()
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

                // Get random word excluding learned words
                var randomWord = await _dictionaryService.GetRandomWordExcludeLearned(userId);

                if (randomWord == null)
                {
                    return NotFound(new { message = "No word found or all words have been learned" });
                }

                // Return all DictionaryResponseDto properties
                return Ok(new { 
                    id = 0, // Add id to match client
                    word = randomWord.Word,
                    phonetic = randomWord.Phonetic,
                    phonetics = randomWord.Phonetics,
                    meanings = randomWord.Meanings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating random word");
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }

        // API endpoint for looking up a word definition
        [HttpGet("lookup/{word}")]
        [Produces("application/json")]
        public async Task<IActionResult> Lookup(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return BadRequest(new { message = "Word cannot be empty" });
            }

            try
            {
                var definition = await _dictionaryService.GetWordDefinition(word);

                if (definition == null)
                {
                    return NotFound(new { message = $"No definition found for word: {word}" });
                }

                return Ok(definition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting word definition: {Word}", word);
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }
    }
}
