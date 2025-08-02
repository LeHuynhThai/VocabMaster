using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.Interfaces.Services.Translation;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [AllowAnonymous] // Allow anonymous access to all endpoints in this controller
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;
        private readonly IVocabularyTranslationService _vocabularyTranslationService;
        private readonly ILogger<TranslationController> _logger;

        public TranslationController(
            ITranslationService translationService,
            IVocabularyTranslationService vocabularyTranslationService,
            ILogger<TranslationController> logger)
        {
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            _vocabularyTranslationService = vocabularyTranslationService ?? throw new ArgumentNullException(nameof(vocabularyTranslationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("word/{word}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> TranslateWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return BadRequest(new { message = "Word parameter is required" });
            }

            try
            {
                _logger.LogInformation("Translating word: {Word}", word);
                var translation = await _translationService.TranslateWord(word);

                if (string.IsNullOrEmpty(translation))
                {
                    _logger.LogWarning("No translation found for word: {Word}", word);
                    return NotFound(new { message = "No translation found" });
                }

                _logger.LogInformation("Successfully translated word: {Word} to {Translation}", word, translation);
                return Ok(translation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating word: {Word}", word);
                return StatusCode(500, new { message = "An error occurred while translating the word" });
            }
        }

        [HttpGet("public/word/{word}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> TranslateWordPublic(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return BadRequest(new { message = "Word parameter is required" });
            }

            try
            {
                _logger.LogInformation("Translating word (public endpoint): {Word}", word);
                var translation = await _translationService.TranslateWord(word);

                if (string.IsNullOrEmpty(translation))
                {
                    _logger.LogWarning("No translation found for word: {Word}", word);
                    return NotFound(new { message = "No translation found" });
                }

                _logger.LogInformation("Successfully translated word: {Word} to {Translation}", word, translation);
                return Ok(translation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating word: {Word}", word);
                return StatusCode(500, new { message = "An error occurred while translating the word" });
            }
        }

        [HttpPost("crawl-all")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CrawlAllTranslations()
        {
            try
            {
                _logger.LogInformation("Starting to crawl all translations");

                var count = await _vocabularyTranslationService.CrawlAllTranslations();

                _logger.LogInformation("Successfully crawled {Count} translations", count);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crawling all translations");
                return StatusCode(500, new { message = "An error occurred while crawling translations" });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint called");
            return Ok(new { message = "Translation controller is working!" });
        }
    }
}