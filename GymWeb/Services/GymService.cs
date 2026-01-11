using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging; //ne trebuie pentru loguri
using GymWeb.Entities;
using System.Text.RegularExpressions;


namespace GymWeb.Services
{
    public class GymService
    {
        private readonly ILogger<GymService> _logger;
        
        private List<Admin> _admins = new();
        private List<Client> _clients = new();
        private List<Sala> _sali = new();
        private List<OfertaAbonament> _oferte = new();
        private List<ClasaFitness> _clase = new();

        public GymService(ILogger<GymService> logger)
        {
            _logger = logger;
            LoadData();
        }

        //autentificare
        public User Login(string user, string pass)
        {
            var admin = _admins.FirstOrDefault(a => a.Username == user && a.Password == pass);
            if (admin != null) {
                _logger.LogInformation($"[LOGIN] Admin logat: {user}");
                return admin;
            }

            var client = _clients.FirstOrDefault(c => c.Username == user && c.Password == pass);
            if (client != null) {
                _logger.LogInformation($"[LOGIN] Client logat: {user}");
                return client;
            }
            
            _logger.LogWarning($"[LOGIN FAIL] Tentativă eșuată pentru user: {user}");
            return null;
        }

        // Modificăm return type din bool în string ca să dăm detalii
        public string RegisterClient(string user, string pass)
        {
            //Setam formatul parolei
            // Minim 8 caractere, o literă mare, o literă mică, o cifră
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{5,}$";
    
            if (!System.Text.RegularExpressions.Regex.IsMatch(pass, pattern))
            {
                _logger.LogWarning($"[WEAK PASS] {user} a încercat o parolă slabă. Respins.");
                return "Minim 5 caractere, o litera mare, o litera mica și o cifră.";
            }

            //Căutăm dacă există deja
            if (_admins.Any(a => a.Username == user) || _clients.Any(c => c.Username == user))
            {
                _logger.LogWarning($"[CLONA] {user} există deja.");
                return $"Numele '{user}' e luat deja.";
            }

            //Cream client
            var newClient = new Client(user, pass);
            _clients.Add(newClient);
            SaveData();
    
            _logger.LogInformation($"[REGISTER] {user} s-a înregistrat cu succes.");
            return null; // Null înseamnă SUCCES
        }
        
        public Client GetClientByUsername(string username) => _clients.FirstOrDefault(c => c.Username == username);
        public List<Sala> GetSali() => _sali;
        public List<OfertaAbonament> GetOferte() => _oferte;
        public List<ClasaFitness> GetClase() => _clase;
        public List<Client> GetAllClients() => _clients;

        //actiunile adminului
        public void AdaugaSala(string nume, string program)
        {
            //Definim formatul orei: "Două cifre:Două cifre - Două cifre:Două cifre
            string pattern = @"^([0-1]\d|2[0-3]):[0-5]\d\s*-\s*([0-1]\d|2[0-3]):[0-5]\d$";

            if (!Regex.IsMatch(program, pattern))
            {
                _logger.LogError($"[FORMAT INVALID] Programul '{program}' nu e bun. Trebuie sa fie intre 00:00-23:59. Ex: 08:30 - 22:00");
                return; 
            }

            var sala = new Sala { Nume = nume, Program = program };
            _sali.Add(sala);
            SaveData();
            _logger.LogInformation($"[ADMIN] Sala adăugată: {nume} | Program: {program}");
        }

        public void StergeSala(Guid id)
        {
            var s = _sali.FirstOrDefault(x => x.Id == id);
            if (s == null) return;
            
            _sali.Remove(s);
            _clase.RemoveAll(c => c.SalaId == s.Id);
            SaveData();
            _logger.LogInformation($"[ADMIN] Sala ștearsă: {s.Nume}");
        }
        
        public void ModificaSala(Guid id, string nume, string program)
        {
            var idx = _sali.FindIndex(x => x.Id == id);
            if (idx == -1) return;
            _sali[idx] = _sali[idx] with { Nume = nume, Program = program };
            SaveData();
        }
        
        public bool AdaugaOferta(string nume, decimal pret, int zile, Guid? salaId)
        {
            //verificam sa nu fie pretul negativ sau zilele negative
            if (pret < 0 || zile < 0)
            {
                _logger.LogError($"[EROARE CRITICĂ] Tentativă fraudă oferta: {nume}, Pret: {pret}. RESPINS.");
                return false;
            }

            var of = new OfertaAbonament { Nume = nume, Pret = pret, ValabilitateZile = zile, SalaId = salaId };
            _oferte.Add(of);
            SaveData();
            _logger.LogInformation($"[ADMIN] Ofertă adăugată: {nume} ({pret} RON)");
            return true;
        }

        public void StergeOferta(Guid id)
        {
            var of = _oferte.FirstOrDefault(x => x.Id == id);
            if (of != null) { _oferte.Remove(of); SaveData(); }
        }

        public bool ProgrameazaClasa(string nume, string antrenor, int capacitate, Guid salaId)
        {
            //verficam capacitatea sa nu fie negativa
            if (capacitate <= 0)
            {
                _logger.LogError($"[EROARE LOGICĂ] Adminul a încercat să facă o clasă '{nume}' cu capacitate {capacitate} inexistenta capacitate trebuie sa fie minim 1.");
                return false;
            }

            //verificam daca sala exista
            if (!_sali.Any(s => s.Id == salaId))
            {
                _logger.LogError($"[EROARE] Se încearcă programarea în sala cu ID {salaId} care nu există.");
                return false;
            }

            var cl = new ClasaFitness 
            { 
                Nume = nume, 
                Antrenor = antrenor, 
                Capacitate = capacitate, 
                Data = DateTime.Now.AddDays(1), 
                SalaId = salaId 
            };

            _clase.Add(cl);
            SaveData();
            _logger.LogInformation($"[CLASA] S-a creat clasa '{nume}' cu {capacitate} locuri. Să curgă transpirația!");
            return true;
        }
        
        public void StergeClasa(Guid id)
        {
             var c = _clase.FirstOrDefault(x => x.Id == id);
             if(c != null) { _clase.Remove(c); SaveData(); }
        }

        //actiuni clienti
        public string CumparaAbonament(string username, Guid ofertaId, Guid? salaTargetId)
        {
            var client = GetClientByUsername(username);
            var oferta = _oferte.FirstOrDefault(o => o.Id == ofertaId);
            
            if (client == null || oferta == null) return "Eroare date.";

            Guid? salaFinalaId = oferta.SalaId;
            string numeSala = "Global";

            if (salaFinalaId == null)
            {
                if (salaTargetId != null)
                {
                    var s = _sali.FirstOrDefault(x => x.Id == salaTargetId);
                    if (s != null) { salaFinalaId = s.Id; numeSala = s.Nume; }
                }
            }
            else
            {
                numeSala = _sali.FirstOrDefault(s => s.Id == salaFinalaId)?.Nume ?? "Necunoscută";
            }

            var ab = new AbonamentClient
            {
                NumeOferta = oferta.Nume, Pret = oferta.Pret, 
                DataStart = DateTime.Now, DataSfarsit = DateTime.Now.AddDays(oferta.ValabilitateZile),
                SalaId = salaFinalaId, NumeSala = numeSala
            };

            client.Abonamente.Add(ab);
            SaveData();
            _logger.LogInformation($"[SHOP] {username} a cumpărat {oferta.Nume}");
            return "Abonament activat!";
        }

        public string RezervaClasa(string username, Guid clasaId)
        {
            var client = GetClientByUsername(username);
            var clasa = _clase.FirstOrDefault(c => c.Id == clasaId);
            if (client == null || clasa == null) return "Eroare.";

            bool areAcces = client.Abonamente.Any(a => a.EsteActiv() && (a.SalaId == null || a.SalaId == clasa.SalaId));
            if (!areAcces) return "Nu ai abonament valid pentru această sală.";
            if (!clasa.AreLocuri()) return "Nu sunt locuri.";
            if (clasa.Rezervari.Any(r => r.UsernameClient == username)) return "Ești deja înscris.";

            var rez = new Rezervare { UsernameClient = username, NumeClasa = clasa.Nume, DataRezervare = DateTime.Now };
            clasa.Rezervari.Add(rez);
            client.RezervariIstoric.Add(rez);
            SaveData();
            return "Rezervat cu succes!";
        }
        
        public string AnuleazaRezervare(string username, Guid rezervareId)
        {
            var client = GetClientByUsername(username);
            if (client == null) return "Eroare.";

            var rez = client.RezervariIstoric.FirstOrDefault(r => r.Id == rezervareId);
            if (rez == null) return "Rezervarea nu există.";

            client.RezervariIstoric.Remove(rez);
            foreach(var c in _clase)
            {
                var rInClasa = c.Rezervari.FirstOrDefault(x => x.Id == rezervareId);
                if (rInClasa != null) c.Rezervari.Remove(rInClasa);
            }
            SaveData();
            return "Rezervare anulată.";
        }

        //salvare date
        private void SaveData()
        {
            try {
                var opt = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText("db_admins.json", JsonSerializer.Serialize(_admins, opt));
                File.WriteAllText("db_clients.json", JsonSerializer.Serialize(_clients, opt));
                File.WriteAllText("db_sali.json", JsonSerializer.Serialize(_sali, opt));
                File.WriteAllText("db_oferte.json", JsonSerializer.Serialize(_oferte, opt));
                File.WriteAllText("db_clase.json", JsonSerializer.Serialize(_clase, opt));
            } catch (Exception ex) { 
                _logger.LogError($"[DB ERROR] Save failed: {ex.Message}");
            }
        }

        private void LoadData()
        {
            List<T> Load<T>(string f) => File.Exists(f) ? JsonSerializer.Deserialize<List<T>>(File.ReadAllText(f)) : new List<T>();
    
            _admins = Load<Admin>("db_admins.json");
            _clients = Load<Client>("db_clients.json");
            _sali = Load<Sala>("db_sali.json");
            _oferte = Load<OfertaAbonament>("db_oferte.json");
            _clase = Load<ClasaFitness>("db_clase.json");

            if (!_admins.Any()) _admins.Add(new Admin("admin", "admin"));
            if (!_clients.Any()) _clients.Add(new Client("client", "client"));
        }
    }
}