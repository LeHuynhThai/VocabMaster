using Microsoft.AspNetCore.Mvc;
using VocabMaster.Models;
using VocabMaster.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace VocabMaster.Controllers
{
    [Authorize]
    public class WordGeneratorController : Controller
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly ILogger<WordGeneratorController> _logger;

        public WordGeneratorController(
            IDictionaryService dictionaryService,
            ILogger<WordGeneratorController> logger)
        {
            _dictionaryService = dictionaryService;
            _logger = logger;
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
                var randomWord = await _dictionaryService.GetRandomWordAsync();
                
                if (randomWord == null)
                {
                    _logger.LogWarning("No word found");
                    ModelState.AddModelError("", "Cannot generate random word. Please try again.");
                    return View("Index");
                }

                ViewBag.RandomWord = randomWord;
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating random word");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetWordDefinition(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return BadRequest("Word cannot be empty");
            }

            try
            {
                var definition = await _dictionaryService.GetWordDefinitionAsync(word);
                
                if (definition == null)
                {
                    _logger.LogWarning($"No definition found for word: {word}");
                    ModelState.AddModelError("", $"No definition found for word: {word}");
                    return View("Index");
                }

                ViewBag.RandomWord = definition;
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting word definition: {word}");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View("Index");
            }
        }
    }
}
