using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Models;
using Microsoft.AspNetCore.Authorization;

namespace VocabMaster.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
