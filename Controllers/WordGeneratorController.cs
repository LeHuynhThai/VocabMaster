using Microsoft.AspNetCore.Mvc;
using VocabMaster.Data;

namespace VocabMaster.Controllers
{
    public class WordGeneratorController : Controller
    {

        private readonly AppDbContext _context;

        public WordGeneratorController(AppDbContext context)
        {
            _context = context;
        }

        private static readonly List<string> Words = new List<string>
        {
            "apple", "banana", "cat", "dog", "elephant", "flower", "guitar", "house", "island", "jungle"
        };

        [HttpGet]
        public IActionResult Index(string randomWord = null)
        {
            ViewBag.RandomWord = randomWord;   
            return View();
        }
        
        [HttpPost]
        public IActionResult GenerateWord()
        {
            var random = new Random();
            var randomWord = Words[random.Next(Words.Count)];
            return RedirectToAction("Index", new { randomWord = randomWord });
        }
    }
}
