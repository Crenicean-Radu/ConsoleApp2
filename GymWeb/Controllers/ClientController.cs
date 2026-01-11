using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GymWeb.Services;
using GymWeb.Entities;
using System;
using System.Linq;

namespace GymWeb.Controllers
{
    public class ClientController : Controller
    {
        private readonly GymService _service;

        public ClientController(GymService service)
        {
            _service = service;
        }

        //Luăm username-ul celui logat
        private string GetMe() => HttpContext.Session.GetString("User");
        private bool IsClient() => HttpContext.Session.GetString("Role") == "Client";

        //profil (Abonamente + Rezervări)
        public IActionResult Index()
        {
            if (!IsClient()) return RedirectToAction("Login", "Account");
            
            var client = _service.GetClientByUsername(GetMe());
            return View(client);
        }

        //Cumpara abonament
        public IActionResult Cumpara()
        {
            if (!IsClient()) return RedirectToAction("Login", "Account");

            // aratam lista de sali si alegi daca sa fie global sau specific
            ViewBag.Sali = _service.GetSali();
            return View(_service.GetOferte());
        }

        [HttpPost]
        public IActionResult CumparaAbonament(Guid ofertaId, Guid? salaTargetId)
        {
            if (!IsClient()) return RedirectToAction("Login", "Account");
            
            string mesaj = _service.CumparaAbonament(GetMe(), ofertaId, salaTargetId);
            
            // Trimitem mesajul (succes sau eroare) către pagină
            TempData["Msg"] = mesaj; 
            
            return RedirectToAction("Index"); 
        }

        //rezervari
        public IActionResult Rezerva()
        {
            if (!IsClient()) return RedirectToAction("Login", "Account");
            //luam salile ca sa le aratam langa clase
            ViewBag.Sali = _service.GetSali();
            return View(_service.GetClase());
        }

        [HttpPost]
        public IActionResult FaRezervare(Guid clasaId)
        {
            if (!IsClient()) return RedirectToAction("Login", "Account");

            string mesaj = _service.RezervaClasa(GetMe(), clasaId);
            TempData["Msg"] = mesaj;

            return RedirectToAction("Index");
        }

        //anularea claselor
        public IActionResult Anuleaza(Guid rezervareId)
        {
            if (!IsClient()) return RedirectToAction("Login", "Account");

            string mesaj = _service.AnuleazaRezervare(GetMe(), rezervareId);
            TempData["Msg"] = mesaj;

            return RedirectToAction("Index");
        }
    }
}