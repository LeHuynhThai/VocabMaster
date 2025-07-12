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
                var wordCount = await _context.Vocabularies.CountAsync();
                if(wordCount == 0)
                {
                    ViewBag.Error = "Không có từ vựng nào trong cơ sở dữ liệu";
                    return View("Index");
                }

                var random = new Random();
                var skipCount = random.Next(0, wordCount);

                var randomWord = await _context.Vocabularies
                    .Skip(skipCount)
                    .Take(1)
                    .FirstOrDefaultAsync();

                if(randomWord != null)
                {
                    ViewBag.RandomWord = randomWord;
                }
                else
                {
                    ViewBag.Error = "Không tìm thấy từ vựng ngẫu nhiên";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
            }

            return View("Index");
        }
    }
}
