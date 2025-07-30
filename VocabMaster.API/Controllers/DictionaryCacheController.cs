using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DictionaryCacheController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly ILogger<DictionaryCacheController> _logger;

        public DictionaryCacheController(
            IDictionaryService dictionaryService,
            ILogger<DictionaryCacheController> logger)
        {
            _dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Caches the definition of a word
        /// </summary>
        /// <param name="word">The word to cache</param>
        /// <returns>Success status</returns>
        [HttpPost("word/{word}")]
        public async Task<IActionResult> CacheWord(string word)
        {
            try
            {
                _logger.LogInformation("Caching definition for word: {Word}", word);
                var result = await _dictionaryService.CacheWordDefinition(word);

                if (result)
                {
                    _logger.LogInformation("Successfully cached definition for word: {Word}", word);
                    return Ok(new { success = true, message = $"Successfully cached definition for word: {word}" });
                }
                else
                {
                    _logger.LogWarning("Failed to cache definition for word: {Word}", word);
                    return BadRequest(new { success = false, message = $"Failed to cache definition for word: {word}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching definition for word: {Word}", word);
                return StatusCode(500, new { success = false, message = "An error occurred while caching the definition" });
            }
        }

        /// <summary>
        /// Caches the definitions of all words in the vocabulary repository
        /// </summary>
        /// <returns>Number of words successfully cached</returns>
        [HttpPost("all")]
        public async Task<IActionResult> CacheAllWords()
        {
            try
            {
                _logger.LogInformation("Starting to cache all vocabulary definitions");
                var result = await _dictionaryService.CacheAllVocabularyDefinitions();

                _logger.LogInformation("Finished caching vocabulary definitions. Success count: {Count}", result);
                return Ok(new { success = true, count = result, message = $"Successfully cached {result} definitions" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching all vocabulary definitions");
                return StatusCode(500, new { success = false, message = "An error occurred while caching the definitions" });
            }
        }
    }
}