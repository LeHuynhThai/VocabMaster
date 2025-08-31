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
        [AllowAnonymous]    
        public async Task<IActionResult> CacheAllVocabularyDefinitions()
        {
            _logger.LogInformation("Bắt đầu quá trình cache tất cả định nghĩa từ vựng");

            var result = await _dictionaryCacheService.CacheAllVocabularyDefinitions();

            _logger.LogInformation("Hoàn thành cache với {count} định nghĩa", result);

            return Ok(new { message = $"Đã cache thành công {result} định nghĩa từ vựng" });
        }
    }
}