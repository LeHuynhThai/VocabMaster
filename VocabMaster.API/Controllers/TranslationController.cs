using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.DTOs;

namespace VocabMaster.API.Controllers
{
    /// <summary>
    /// Controller for translation operations
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;
        private readonly ILogger<TranslationController> _logger;
        private readonly IMemoryCache _cache;
        private const string TranslationCacheKey = "Translation_";
        private const int CacheExpirationMinutes = 60;

        /// <summary>
        /// Initializes a new instance of the TranslationController
        /// </summary>
        /// <param name="translationService">Service for translation operations</param>
        /// <param name="logger">Logger for the controller</param>
        /// <param name="cache">Memory cache for improved performance</param>
        public TranslationController(
            ITranslationService translationService,
            ILogger<TranslationController> logger,
            IMemoryCache cache = null)
        {
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

        /// <summary>
        /// Translates text from English to Vietnamese
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <returns>Translated text</returns>
        /// <response code="200">Returns the translated text</response>
        /// <response code="400">If the text is null or empty</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpGet("en-to-vi")]
        [ProducesResponseType(typeof(TranslationResponseDto), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> TranslateEnglishToVietnamese([FromQuery] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest(new { message = "Text cannot be empty" });
            }

            try
            {
                // Try to get from cache first
                string cacheKey = $"{TranslationCacheKey}en_vi_{text}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out TranslationResponseDto cachedTranslation))
                {
                    _logger.LogInformation("Retrieved translation from cache for text: {Text}", text);
                    return Ok(cachedTranslation);
                }

                _logger.LogInformation("Translating text from English to Vietnamese: {Text}", text);
                var translation = await _translationService.TranslateEnglishToVietnamese(text);

                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _cache.Set(cacheKey, translation, cacheOptions);
                }

                return Ok(translation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating text: {Text}", text);
                return StatusCode(500, new { message = "An error occurred during translation" });
            }
        }

        /// <summary>
        /// Translates text between specified languages
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="source">Source language code (default: "en")</param>
        /// <param name="target">Target language code (default: "vi")</param>
        /// <returns>Translated text</returns>
        /// <response code="200">Returns the translated text</response>
        /// <response code="400">If the text is null or empty</response>
        /// <response code="500">If an error occurs during processing</response>
        [HttpPost("translate")]
        [ProducesResponseType(typeof(TranslationResponseDto), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Q))
            {
                return BadRequest(new { message = "Text cannot be empty" });
            }

            // Use default values if not provided
            var source = string.IsNullOrEmpty(request.Source) ? "en" : request.Source;
            var target = string.IsNullOrEmpty(request.Target) ? "vi" : request.Target;

            try
            {
                // Try to get from cache first
                string cacheKey = $"{TranslationCacheKey}{source}_{target}_{request.Q}";
                if (_cache != null && _cache.TryGetValue(cacheKey, out TranslationResponseDto cachedTranslation))
                {
                    _logger.LogInformation("Retrieved translation from cache for text: {Text}", request.Q);
                    return Ok(cachedTranslation);
                }

                _logger.LogInformation("Translating text from {Source} to {Target}: {Text}", source, target, request.Q);
                var translation = await _translationService.Translate(request.Q, source, target);

                // Cache the result
                if (_cache != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes))
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _cache.Set(cacheKey, translation, cacheOptions);
                }

                return Ok(translation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating text: {Text}", request.Q);
                return StatusCode(500, new { message = "An error occurred during translation" });
            }
        }
    }
} 