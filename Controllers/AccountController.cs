using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Models;

namespace VocabMaster.Controllers
{
    public class AccountController : Controller
    {

        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string Name)
        {
            var user =  await _context.Users.FirstOrDefaultAsync(u => u.Name == Name);
            if (user != null)
            {
                HttpContext.Session.SetString("Name", user.Name);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập không đúng";
                return View();
            }    
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if(await _context.Users.AnyAsync(u => u.Name == user.Name))
            {
                ModelState.AddModelError("Name", "Tên đăng nhập đã tồn tại");
                return View();
            }
            else
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }    
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
