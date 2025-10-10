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

        public DictionaryCacheController(
            IDictionaryCacheService dictionaryCacheService)
        {   
            _dictionaryCacheService = dictionaryCacheService;
        }
        
        [HttpPost("cache-all")]
        [AllowAnonymous]    
        public async Task<IActionResult> CacheAllVocabularyDefinitions()
        {

            var result = await _dictionaryCacheService.CacheAllVocabularyDefinitions();


            return Ok(new { message = $"Đã cache thành công {result} định nghĩa từ vựng" });
        }
    }
}