using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GymWeb.Services;
using GymWeb.Entities;

namespace GymWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly GymService _service;

        public AccountController(GymService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Asta afișează pagina de login
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _service.Login(username, password);
            
            if (user == null)
            {
                ViewBag.Error = "Ai greșit userul sau parola, patroane!";
                return View();
            }

            // slavam informatii user
            HttpContext.Session.SetString("User", user.Username);
            HttpContext.Session.SetString("Role", user.Rol);
            
            if (user is Admin) 
                return RedirectToAction("Index", "Admin");
            else 
                return RedirectToAction("Index", "Client");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Ștergem sesiunea
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            // Apelăm service-ul și primim mesajul (sau null dacă e bine)
            string mesajEroare = _service.RegisterClient(username, password);
    
            // Dacă mesajEroare nu e null, înseamnă că avem o problemă
            if (mesajEroare != null)
            {
                ViewBag.Error = mesajEroare; // Trimitem mesajul exact în pagină
                return View(); // Îl ținem pe pagina de înregistrare să mai încerce
            }

           
            return RedirectToAction("Login");
        }
        
    }
}