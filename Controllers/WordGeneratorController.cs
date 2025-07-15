using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Models;
using VocabMaster.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace VocabMaster.Controllers
{
    public class WordGeneratorController : Controller
    {

        private readonly IDictionaryService _dictionaryService; // Dictionary service

        // Constructor
        public WordGeneratorController(IDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Generate word
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GenerateWord()
        {
            try
            {
                var randomWord = await _dictionaryService.GetRandomWordAsync(); // Get random word
                if(randomWord != null) // If random word exists
                {
                    ViewBag.RandomWord = randomWord; // display random word
                }
                else
                {
                    ViewBag.Error = "No word found"; // display error
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
