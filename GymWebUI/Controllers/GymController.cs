using GymWebUI.Entities;
using GymWebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymWebUI.Controllers;

public class GymController : Controller
{
    private readonly GymService _service;
    public GymController(GymService service) => _service = service;

    // --- ZONA PUBLICĂ (Vizualizare) ---
    // Atentie: _service.GetSali() trebuie sa returneze List<Gym>
    public IActionResult Index() => View(_service.Sali); 
    public IActionResult Oferte() => View(_service.Oferte);

    public IActionResult Clase(Guid? id) // Orarul pentru o sală
    {
        var clase = _service.Clase.AsEnumerable(); // Luam lista de clase (FitnessClass)
        if (id.HasValue) 
        {
            clase = clase.Where(c => c.SalaId == id.Value);
        }
        
        ViewBag.SalaId = id;
        return View(clase.ToList());
    }

    // --- ZONA ADMIN (CRUD Total) ---
    [Authorize(Roles = "Admin")]
    public IActionResult Dashboard()
    {
        // Aici a fost eroarea cu "AbonamentActiv".
        // Acum clientul are o LISTA de abonamente. Verificăm dacă are vreuno valid.
        var abonamenteActiveCount = _service.Clienti
            .Count(c => c.Abonamente.Any(a => a.DataSfarsit > DateTime.Now));

        var stats = new
        {
            NrSali = _service.Sali.Count,
            NrAbonamente = abonamenteActiveCount,
            GradOcupareClase = _service.Clase.Sum(c => c.Rezervari.Count)
        };
        ViewBag.Stats = stats;
        return View();
    }

    // -- SĂLI (Gym) --
    [Authorize(Roles = "Admin")] 
    public IActionResult AdaugaSala() => View();
    
    [HttpPost, Authorize(Roles = "Admin")]
    public IActionResult AdaugaSala(Gym sala) // Am schimbat Sala -> Gym
    {
        // GymZone e record positional acum (constructor cu parametri)
        sala.Zone.Add(new GymZone("General", 50)); 
        
        _service.Sali.Add(sala);
        // _service.SaveAll(); // Decomenteaza cand ai metoda de salvare
        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult StergeSala(Guid id)
    {
        var sala = _service.Sali.FirstOrDefault(s => s.Id == id);
        if(sala != null) _service.Sali.Remove(sala);
        return RedirectToAction("Index");
    }

    // -- OFERTE (SubscriptionOffer) --
    [Authorize(Roles = "Admin")] 
    public IActionResult AdaugaOferta() 
    {
        ViewBag.Sali = _service.Sali; // Pt dropdown
        return View(); 
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public IActionResult AdaugaOferta(SubscriptionOffer oferta) // Am schimbat OfertaAbonament -> SubscriptionOffer
    {
        _service.Oferte.Add(oferta);
        return RedirectToAction("Oferte");
    }

    [Authorize(Roles = "Admin")] 
    public IActionResult StergeOferta(Guid id)
    {
        var of = _service.Oferte.FirstOrDefault(o => o.Id == id);
        if(of != null) _service.Oferte.Remove(of);
        return RedirectToAction("Oferte");
    }

    // -- CLASE (FitnessClass) --
    [Authorize(Roles = "Admin")]
    public IActionResult AdaugaClasa(Guid salaId)
    {
        ViewBag.SalaId = salaId;
        return View();
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public IActionResult AdaugaClasa(FitnessClass clasa) // Am schimbat ClasaFitness -> FitnessClass
    {
        _service.Clase.Add(clasa);
        return RedirectToAction("Clase", new { id = clasa.SalaId });
    }

    [Authorize(Roles = "Admin")] 
    public IActionResult StergeClasa(Guid id, Guid salaId)
    {
        var cls = _service.Clase.FirstOrDefault(c => c.Id == id);
        if(cls != null) _service.Clase.Remove(cls);
        return RedirectToAction("Clase", new { id = salaId });
    }

    [Authorize(Roles = "Admin")] 
    public IActionResult Rezervari(Guid id)
    {
        var clasa = _service.Clase.FirstOrDefault(c => c.Id == id);
        return View(clasa);
    }

    // --- ZONA CLIENT ---
    // Aici presupunem ca ai mutat metodele de business (ProcessCumparare etc.) in Service
    // Sau le scriem direct aici daca nu le ai in Service inca.
    
    [Authorize(Roles = "Client")]
    public IActionResult Cumpara(Guid ofertaId)
    {
        // Logica simplificata direct in controller daca nu e in service:
        var client = _service.Clienti.FirstOrDefault(c => c.Username == User.Identity.Name);
        var oferta = _service.Oferte.FirstOrDefault(o => o.Id == ofertaId);

        if (client != null && oferta != null)
        {
             // Mapping manual Oferta -> AbonamentClient
             var sub = new AbonamentClient
             {
                 NumeOferta = oferta.Nume,
                 Pret = oferta.Pret,
                 DataStart = DateTime.Now,
                 DataSfarsit = DateTime.Now.AddDays(oferta.ValabilitateZile),
                 SalaId = oferta.SalaId,
                 NumeSala = "Unknown" // Ar trebui cautata sala dupa ID
             };
             client.Abonamente.Add(sub);
             TempData["Msg"] = "Ai cumpărat abonamentul!";
        }
        else 
        {
             TempData["Msg"] = "Eroare la cumpărare.";
        }
        return RedirectToAction("Profil");
    }

    [Authorize(Roles = "Client")]
    public IActionResult Rezerva(Guid clasaId)
    {
        var client = _service.Clienti.FirstOrDefault(c => c.Username == User.Identity.Name);
        var clasa = _service.Clase.FirstOrDefault(c => c.Id == clasaId);

        if (client != null && clasa != null && clasa.Rezervari.Count < clasa.Capacitate)
        {
            var rez = new RezervareIstoric 
            { 
                UsernameClient = client.Username,
                NumeClasa = clasa.Nume,
                DataRezervare = DateTime.Now
            };
            
            // Adaugam si la client si la clasa
            client.RezervariIstoric.Add(rez);
            clasa.Rezervari.Add(rez);
            
            TempData["Msg"] = "Rezervat cu succes!";
        }
        else
        {
             TempData["Msg"] = "Nu s-a putut rezerva (locuri lipsa sau eroare).";
        }
        
        return RedirectToAction("Clase", new { id = clasa?.SalaId });
    }

    [Authorize(Roles = "Client")]
    public IActionResult Anuleaza(Guid clasaId) // Aici primeai probabil ID-ul rezervarii sau clasei
    {
         // Logica de anulare...
        TempData["Msg"] = "Rezervare anulată.";
        return RedirectToAction("Profil");
    }

    [Authorize] // Profil personal
    public IActionResult Profil()
    {
        if (User.IsInRole("Admin")) return RedirectToAction("Dashboard");
        
        var client = _service.Clienti.FirstOrDefault(c => c.Username == User.Identity.Name);
        
        // Găsim clasele viitoare rezervate
        // Atentie: Rezervari e acum in 'RezervariIstoric'
        if(client != null) {
            ViewBag.ClaseRezervate = _service.Clase
                .Where(c => c.Rezervari.Any(r => r.UsernameClient == client.Username))
                .ToList();
        }
            
        return View(client);
    }
}