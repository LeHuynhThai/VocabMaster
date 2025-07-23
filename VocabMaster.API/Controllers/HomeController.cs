using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.API.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountService _accountService;

        public HomeController(
            ILogger<HomeController> logger,
            IAccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _accountService.GetCurrentUser();
            return View(user);
        }
    }
}
