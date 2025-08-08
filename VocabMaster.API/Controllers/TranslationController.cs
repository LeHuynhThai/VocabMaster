using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Services.Translation;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [AllowAnonymous] // Allow anonymous access to all endpoints in this controller
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;
        private readonly ILogger<TranslationController> _logger;

        public TranslationController(
            TranslationService translationService,
            ILogger<TranslationController> logger)
        {
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("crawl-all")]
        public async Task<IActionResult> CrawlAllTranslations()
        {
            try
            {
                _logger.LogInformation("Starting to crawl all translations");

                var count = await _translationService.CrawlAllTranslations();

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