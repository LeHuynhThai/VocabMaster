using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Models;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Controllers
{
    public class WordGeneratorController : Controller
    {

        private readonly IVocabularyService _vocabularyService; // Vocabulary service

        // Constructor
        public WordGeneratorController(IVocabularyService vocabularyService)
        {
            _vocabularyService = vocabularyService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        // Generate word
        [HttpPost]
        public async Task<IActionResult> GenerateWord()
        {
            try 
            {
                var randomWord = await _vocabularyService.GetRandomVocabularyAsync(); // Get random vocabulary
                if(randomWord != null) // If random word exists
                {
                    ViewBag.RandomWord = randomWord; // display random word
                }
                else
                {
                    ViewBag.Error = "No vocabulary found"; // display error
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred: " + ex.Message; // display error
            }

            return View("Index"); // return view
        }
    }
}
