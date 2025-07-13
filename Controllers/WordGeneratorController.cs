using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class WordGeneratorController : Controller
    {

        private readonly AppDbContext _context;

        public WordGeneratorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> GenerateWord()
        {
            try 
            {
                // Lấy tổng số từ vựng trong bảng Vocabularies
                var wordCount = await _context.Vocabularies.CountAsync();
                // Nếu không có từ vựng nào trong bảng Vocabularies, hiển thị thông báo lỗi
                if(wordCount == 0)
                {
                    ViewBag.Error = "Không có từ vựng nào trong cơ sở dữ liệu";
                    return View("Index");
                }
                // Tạo số ngẫu nhiên
                var random = new Random();
                // Tạo số ngẫu nhiên trong khoảng từ 0 đến tổng số từ vựng
                var skipCount = random.Next(0, wordCount);
                // Lấy từ vựng ngẫu nhiên từ bảng Vocabularies
                var randomWord = await _context.Vocabularies
                    .Skip(skipCount) // bỏ qua số lượng từ vựng ngẫu nhiên
                    .Take(1) // lấy 1 từ vựng kế tiếp
                    .FirstOrDefaultAsync(); // lấy từ vựng đầu tiên

                // Nếu từ vựng ngẫu nhiên tồn tại, hiển thị từ vựng ngẫu nhiên
                if(randomWord != null)
                {
                    ViewBag.RandomWord = randomWord;
                }
                // Nếu từ vựng ngẫu nhiên không tồn tại, hiển thị thông báo lỗi
                else
                {
                    ViewBag.Error = "Không tìm thấy từ vựng ngẫu nhiên";
                }
            }
            // Nếu có lỗi xảy ra, hiển thị thông báo lỗi
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
            }

            return View("Index");
        }
    }
}
