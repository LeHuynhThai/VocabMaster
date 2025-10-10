using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;
        public TranslationController(
            TranslationService translationService)
        {
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        }

        [HttpPost("crawl-all")]
        public async Task<IActionResult> CrawlAllTranslations()
        {
            try
            {

                var count = await _translationService.CrawlAllTranslations();

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while crawling translations" });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Translation controller is working!" });
        }
    }
}