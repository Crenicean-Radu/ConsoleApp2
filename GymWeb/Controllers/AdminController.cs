using Microsoft.AspNetCore.Mvc;
using GymWeb.Services;
using GymWeb.Entities;
using Microsoft.AspNetCore.Http;
using System;

namespace GymWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly GymService _service;

        public AdminController(GymService service)
        {
            _service = service;
        }

        // Verificăm dacă e Admin.
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // MONITORIZARE
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(_service); 
        }

        //GESTIUNE SĂLI
        public IActionResult Sali()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(_service.GetSali());
        }

        [HttpPost]
        public IActionResult AdaugaSala(string nume, string program)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            
            // Aici la Sala e void in service
            _service.AdaugaSala(nume, program);
            TempData["Succes"] = "Sala a fost inaugurată! 🏢";
            
            return RedirectToAction("Sali");
        }

        public IActionResult StergeSala(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _service.StergeSala(id);
            TempData["Succes"] = "Sala a fost demolată din sistem.";
            return RedirectToAction("Sali");
        }

        [HttpPost]
        public IActionResult ModificaSala(Guid id, string nume, string program)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _service.ModificaSala(id, nume, program);
            TempData["Succes"] = "Sala a fost renovată (modificată).";
            return RedirectToAction("Sali");
        }

        //GESTIUNE OFERTE
        public IActionResult Oferte()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.Sali = _service.GetSali(); 
            return View(_service.GetOferte());
        }

        [HttpPost]
        public IActionResult AdaugaOferta(string nume, decimal pret, int zile, Guid? salaId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
    
            if (salaId == Guid.Empty) salaId = null;
            
            // LOGICA DE VALIDARE
            bool succes = _service.AdaugaOferta(nume, pret, zile, salaId);

            if (!succes)
            {
                TempData["Eroare"] = "Stop joc! Preț sau zile negative. Nu facem caritate aici.";
            }
            else
            {
                TempData["Succes"] = "Ofertă lansată pe piață! 💸";
            }
    
            return RedirectToAction("Oferte");
        }

        public IActionResult StergeOferta(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _service.StergeOferta(id);
            TempData["Succes"] = "Oferta a expirat (ștearsă).";
            return RedirectToAction("Oferte");
        }

        //GESTIUNE CLASE
        public IActionResult Clase()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.Sali = _service.GetSali();
            return View(_service.GetClase());
        }

        [HttpPost]
        public IActionResult ProgrameazaClasa(string nume, string antrenor, int capacitate, Guid salaId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // VALIDARE CAPACITATE & SALA
            bool reusit = _service.ProgrameazaClasa(nume, antrenor, capacitate, salaId);

            if (!reusit)
            {
                TempData["Eroare"] = "Eroare: Ori ai băgat capacitate aiurea (Microbuz Magic), ori sala nu există!";
            }
            else
            {
                TempData["Succes"] = "Clasa a fost programată. Să curgă transpirația! 🏋️";
            }

            return RedirectToAction("Clase");
        }

        public IActionResult StergeClasa(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _service.StergeClasa(id);
            TempData["Succes"] = "Clasa anulată.";
            return RedirectToAction("Clase");
        }
        
        
    }
}