using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers
{
    /// <summary>
    /// Controller for translation operations
    /// </summary>
    [ApiController]
    [AllowAnonymous] // Allow anonymous access to all endpoints in this controller
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationCrawlerService _translationCrawlerService;
        private readonly ILogger<TranslationController> _logger;

        /// <summary>
        /// Initializes a new instance of the TranslationController
        /// </summary>
        /// <param name="translationCrawlerService">Service for translation operations</param>
        /// <param name="logger">Logger for the controller</param>
        public TranslationController(
            ITranslationCrawlerService translationCrawlerService,
            ILogger<TranslationController> logger)
        {
            _translationCrawlerService = translationCrawlerService ?? throw new ArgumentNullException(nameof(translationCrawlerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Translates a single English word to Vietnamese
        /// </summary>
        /// <param name="word">The English word to translate</param>
        /// <returns>The Vietnamese translation</returns>
        /// <response code="200">Returns the Vietnamese translation</response>
        /// <response code="400">If the word parameter is null or empty</response>
        /// <response code="404">If no translation is found</response>
        /// <response code="500">If an error occurs during processing</response>
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
                var translation = await _translationCrawlerService.TranslateWord(word);
                
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

        /// <summary>
        /// Translates a single English word to Vietnamese (public endpoint, no authentication required)
        /// </summary>
        /// <param name="word">The English word to translate</param>
        /// <returns>The Vietnamese translation</returns>
        /// <response code="200">Returns the Vietnamese translation</response>
        /// <response code="400">If the word parameter is null or empty</response>
        /// <response code="404">If no translation is found</response>
        /// <response code="500">If an error occurs during processing</response>
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
                var translation = await _translationCrawlerService.TranslateWord(word);
                
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

        /// <summary>
        /// Crawls Vietnamese translations for all English vocabulary in the database
        /// </summary>
        /// <returns>The number of translations successfully crawled</returns>
        /// <response code="200">Returns the number of translations successfully crawled</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpPost("crawl-all")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CrawlAllTranslations()
        {
            try
            {
                _logger.LogInformation("Starting to crawl all translations");
                
                // Use the actual service to crawl translations
                var count = await _translationCrawlerService.CrawlAllTranslations();
                
                _logger.LogInformation("Successfully crawled {Count} translations", count);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crawling all translations");
                return StatusCode(500, new { message = "An error occurred while crawling translations" });
            }
        }

        /// <summary>
        /// Simple test endpoint to check if the controller is working
        /// </summary>
        /// <returns>A simple test message</returns>
        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint called");
            return Ok(new { message = "Translation controller is working!" });
        }
    }
} 