using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Services.Translation;

namespace VocabMaster.API.Controllers
{
    // Controller quản lý các API liên quan đến dịch thuật (translation)
    // Cho phép truy cập ẩn danh, dùng để crawl dữ liệu dịch và kiểm tra hoạt động
    [ApiController]
    [AllowAnonymous] // Allow anonymous access to all endpoints in this controller
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TranslationController : ControllerBase
    {
        // Service xử lý logic dịch thuật
        private readonly TranslationService _translationService;
        // Ghi log cho controller
        private readonly ILogger<TranslationController> _logger;

        // Hàm khởi tạo controller, inject các service cần thiết
        public TranslationController(
            TranslationService translationService,
            ILogger<TranslationController> logger)
        {
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crawl toàn bộ dữ liệu dịch thuật (chạy background crawl)
        /// </summary>
        [HttpPost("crawl-all")]
        public async Task<IActionResult> CrawlAllTranslations()
        {
            try
            {
                _logger.LogInformation("Starting to crawl all translations");

                // Gọi service để crawl toàn bộ dữ liệu dịch
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

        /// <summary>
        /// Endpoint test kiểm tra hoạt động của controller
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint called");
            return Ok(new { message = "Translation controller is working!" });
        }
    }
}