using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.API.Controllers
{
    /// <summary>
    /// Controller quản lý cache từ điển
    /// Xử lý các chức năng liên quan đến việc cache định nghĩa từ vựng
    /// để tăng tốc độ truy xuất và giảm tải cho API từ điển bên ngoài
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DictionaryCacheController : ControllerBase
    {
        // Service xử lý cache từ điển
        private readonly IDictionaryCacheService _dictionaryCacheService;
        // Logger để ghi log các hoạt động
        private readonly ILogger<DictionaryCacheController> _logger;

        /// <summary>
        /// Constructor - Khởi tạo DictionaryCacheController với các dependency cần thiết
        /// </summary>
        /// <param name="dictionaryCacheService">Service xử lý cache từ điển</param>
        /// <param name="logger">Logger để ghi log</param>
        public DictionaryCacheController(
            IDictionaryCacheService dictionaryCacheService,
            ILogger<DictionaryCacheController> logger)
        {
            // Gán các dependency được inject vào các field
            _dictionaryCacheService = dictionaryCacheService;
            _logger = logger;
        }

        /// <summary>
        /// API cache tất cả định nghĩa từ vựng
        /// Lấy tất cả từ vựng trong hệ thống và cache định nghĩa của chúng
        /// để tăng tốc độ tra cứu sau này
        /// </summary>
        /// <returns>Số lượng định nghĩa đã được cache thành công</returns>
        [HttpPost("cache-all")]
        [AllowAnonymous] // Cho phép truy cập mà không cần xác thực (có thể dùng cho admin hoặc scheduled job)
        public async Task<IActionResult> CacheAllVocabularyDefinitions()
        {
            // Ghi log bắt đầu quá trình cache
            _logger.LogInformation("Bắt đầu quá trình cache tất cả định nghĩa từ vựng");

            // Gọi service để thực hiện cache tất cả định nghĩa từ vựng
            var result = await _dictionaryCacheService.CacheAllVocabularyDefinitions();

            // Ghi log kết quả hoàn thành
            _logger.LogInformation("Hoàn thành cache với {count} định nghĩa", result);

            // Trả về kết quả thành công với số lượng định nghĩa đã cache
            return Ok(new { message = $"Đã cache thành công {result} định nghĩa từ vựng" });
        }
    }
}