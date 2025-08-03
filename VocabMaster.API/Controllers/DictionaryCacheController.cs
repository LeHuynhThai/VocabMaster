using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DictionaryCacheController : ControllerBase
    {
        private readonly IDictionaryCacheService _dictionaryCacheService;
        private readonly ILogger<DictionaryCacheController> _logger;

        public DictionaryCacheController(
            IDictionaryCacheService dictionaryCacheService,
            ILogger<DictionaryCacheController> logger)
        {
            _dictionaryCacheService = dictionaryCacheService;
            _logger = logger;
        }

        [HttpPost("cache-all")]
        [Authorize]
        public async Task<IActionResult> CacheAllVocabularyDefinitions()
        {
            _logger.LogInformation("Starting cache all vocabulary definitions process");
            var result = await _dictionaryCacheService.CacheAllVocabularyDefinitions();
            _logger.LogInformation("Completed caching with {count} definitions", result);
            return Ok(new { message = $"Successfully cached {result} vocabulary definitions" });
        }
    }
}