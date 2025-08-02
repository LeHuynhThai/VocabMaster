using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DictionaryCacheController : ControllerBase
    {
        private readonly IDictionaryCacheService _dictionaryCacheService;
        private readonly IDictionaryLookupService _dictionaryLookupService;
        private readonly IRandomWordService _randomWordService;
        private readonly ILogger<DictionaryCacheController> _logger;

        public DictionaryCacheController(
            IDictionaryCacheService dictionaryCacheService,
            IDictionaryLookupService dictionaryLookupService,
            IRandomWordService randomWordService,
            ILogger<DictionaryCacheController> logger)
        {
            _dictionaryCacheService = dictionaryCacheService;
            _dictionaryLookupService = dictionaryLookupService;
            _randomWordService = randomWordService;
            _logger = logger;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomWord()
        {
            var result = await _randomWordService.GetRandomWord();
            if (result == null)
            {
                return NotFound(new { message = "No random word found" });
            }
            return Ok(result);
        }

        [HttpGet("random/exclude-learned/{userId}")]
        public async Task<IActionResult> GetRandomWordExcludeLearned(int userId)
        {
            var result = await _randomWordService.GetRandomWordExcludeLearned(userId);
            if (result == null)
            {
                return NotFound(new { message = "No random word found" });
            }
            return Ok(result);
        }

        [HttpGet("word/{word}")]
        public async Task<IActionResult> GetWordDefinition(string word)
        {
            var result = await _dictionaryLookupService.GetWordDefinition(word);
            if (result == null)
            {
                return NotFound(new { message = $"No definition found for word: {word}" });
            }
            return Ok(result);
        }

        [HttpGet("cache/{word}")]
        public async Task<IActionResult> GetWordDefinitionFromCache(string word)
        {
            var result = await _dictionaryLookupService.GetWordDefinitionFromCache(word);
            if (result == null)
            {
                return NotFound(new { message = $"No definition found for word: {word}" });
            }
            return Ok(result);
        }

        [HttpPost("cache/{word}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CacheWordDefinition(string word)
        {
            var result = await _dictionaryLookupService.CacheWordDefinition(word);
            if (!result)
            {
                return BadRequest(new { message = $"Failed to cache definition for word: {word}" });
            }
            return Ok(new { message = $"Successfully cached definition for word: {word}" });
        }

        [HttpPost("cache-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CacheAllVocabularyDefinitions()
        {
            var result = await _dictionaryLookupService.CacheAllVocabularyDefinitions();
            return Ok(new { message = $"Successfully cached {result} vocabulary definitions" });
        }
    }
}