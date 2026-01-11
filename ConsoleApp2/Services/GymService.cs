using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ConsoleApp2.Entities;

namespace ConsoleApp2.Services
{
    public class GymService
    {
        private readonly ILogger<GymService> _logger;

        private List<Admin> _admins = new();
        private List<Client> _clients = new();
        private List<Sala> _sali = new();
        private List<OfertaAbonament> _oferte = new();
        private List<ClasaFitness> _clase = new();

        public User CurrentUser { get; private set; }

        public GymService(ILogger<GymService> logger)
        {
            _logger = logger;
            LoadData();
        }

        //autentificare
        public bool Login(string user, string pass)
        {
            var admin = _admins.FirstOrDefault(a => a.Username == user && a.Password == pass);
            if (admin != null) 
            { CurrentUser = admin; return true; }

            var client = _clients.FirstOrDefault(c => c.Username == user && c.Password == pass);
            if (client != null) 
            { CurrentUser = client; return true; }

            return false;
        }

        public void Logout() => CurrentUser = null;

        //luare valorile salvate
        public IReadOnlyList<Sala> GetSali() => _sali.AsReadOnly();
        public IReadOnlyList<OfertaAbonament> GetOferte() => _oferte.AsReadOnly();
        public IReadOnlyList<ClasaFitness> GetClase() => _clase.AsReadOnly();
        public Client GetCurrentClient() => CurrentUser as Client;

       
        public void AdaugaSala(string nume, string program, List<Zona> zone)
        {
            var sala = new Sala { Nume = nume, Program = program, Zone = zone };
            _sali.Add(sala);
            SaveData();
            _logger.LogInformation($"[ADMIN] Sala adăugată: {nume}");
        }

        public void StergeSala(int index)
        {
            if (index < 0 || index >= _sali.Count) return;
            var s = _sali[index];
            _sali.RemoveAt(index);
            // Ștergem și clasele
            _clase.RemoveAll(c => c.SalaId == s.Id);
            SaveData();
            _logger.LogInformation($"[ADMIN] Sala ștearsă: {s.Nume}");
        }

        public void ModificaSala(int index, string numeNou, string programNou)
        {
            if (index < 0 || index >= _sali.Count) return;
            var old = _sali[index];
            var updated = old with { Nume = numeNou, Program = programNou };
            _sali[index] = updated;
            SaveData();
        }

        //metode oferte
        public void AdaugaOferta(string nume, decimal pret, int zile, Guid? salaId)
        {
            var of = new OfertaAbonament { Nume = nume, Pret = pret, ValabilitateZile = zile, SalaId = salaId };
            _oferte.Add(of);
            SaveData();
        }

        public void StergeOferta(int index)
        {
            if (index < 0 || index >= _oferte.Count) return;
            _oferte.RemoveAt(index);
            SaveData();
        }

        //metode clase
        public void ProgrameazaClasa(string nume, string antrenor, int capacitate, Guid salaId)
        {
            var cl = new ClasaFitness 
            { 
                Nume = nume, Antrenor = antrenor, Capacitate = capacitate, 
                Data = DateTime.Now.AddDays(1), SalaId = salaId 
            };
            _clase.Add(cl);
            SaveData();
        }

        public void StergeClasa(int index)
        {
            if (index < 0 || index >= _clase.Count) return;
            _clase.RemoveAt(index);
            SaveData();
        }

        public void ModificaClasa(int index, string nume, string antrenor, int cap)
        {
            if (index < 0 || index >= _clase.Count) return;
            var old = _clase[index];
            var updated = old with { Nume = nume, Antrenor = antrenor, Capacitate = cap };
            _clase[index] = updated;
            SaveData();
        }

        public void PrintMonitorizare()
        {
             Console.WriteLine($"\n[STATS] Total Abonamente Vândute: {_clients.Sum(c => c.Abonamente.Count)}");
             foreach(var c in _clase)
             {
                 var sala = _sali.FirstOrDefault(s => s.Id == c.SalaId)?.Nume ?? "?";
                 Console.WriteLine($"{c.Nume} ({sala}): {c.Rezervari.Count}/{c.Capacitate}");
             }
        }

        //metoded client

        public string CumparaAbonament(int indexOferta, int indexSalaDorita)
        {
            if (CurrentUser is not Client client) return "Nu ești client.";
            if (indexOferta < 0 || indexOferta >= _oferte.Count) return "Ofertă invalidă.";

            var of = _oferte[indexOferta];
            
            //Dacă oferta e generică (SalaId == null), clientul ALEGE sala.
            //Dacă oferta e legată deja luăm sala ofertei.
            
            Guid? salaFinalaId = of.SalaId;
            string numeSala = "Global / All Access";

            if (salaFinalaId == null)
            {
                //Clientul trebuie să aleagă sala
                if (indexSalaDorita >= 0 && indexSalaDorita < _sali.Count)
                {
                    var s = _sali[indexSalaDorita];
                    salaFinalaId = s.Id;
                    numeSala = s.Nume;
                }
            }
            else
            {
                numeSala = _sali.FirstOrDefault(s => s.Id == salaFinalaId)?.Nume ?? "Necunoscută";
            }

            var ab = new AbonamentClient
            {
                NumeOferta = of.Nume, Pret = of.Pret, 
                DataStart = DateTime.Now, DataSfarsit = DateTime.Now.AddDays(of.ValabilitateZile),
                SalaId = salaFinalaId, NumeSala = numeSala
            };

            client.Abonamente.Add(ab);
            SaveData();
            _logger.LogInformation($"[BUY] {client.Username} -> {of.Nume} ({numeSala})");
            return "Abonament activat!";
        }

        public string RezervaClasa(int indexClasa)
        {
            if (CurrentUser is not Client client) return "Nu ești client.";
            if (indexClasa < 0 || indexClasa >= _clase.Count) return "Index invalid.";

            var clasa = _clase[indexClasa];

            // Validare abonament
            bool areAcces = client.Abonamente.Any(a => 
                a.EsteActiv() && (a.SalaId == null || a.SalaId == clasa.SalaId)
            );

            if (!areAcces) 
            {
                _logger.LogWarning($"[ACCES DENIED] {client.Username} la sala {_sali.FirstOrDefault(s=>s.Id==clasa.SalaId)?.Nume}");
                return "EROARE: Nu ai abonament activ pentru sala unde se ține clasa!";
            }

            if (!clasa.AreLocuri()) return "Clasa e plină.";
            if (clasa.Rezervari.Any(r => r.UsernameClient == client.Username)) return "Ai rezervat deja.";

            var rez = new Rezervare 
            { 
                UsernameClient = client.Username, 
                NumeClasa = clasa.Nume, 
                DataRezervare = DateTime.Now 
            };

            clasa.Rezervari.Add(rez);
            client.RezervariIstoric.Add(rez);
            SaveData();
            return "Rezervare confirmată!";
        }

        public string AnuleazaRezervare(int indexRezervare)
        {
            if (CurrentUser is not Client client) return "Nu ești client.";
            if (indexRezervare < 0 || indexRezervare >= client.RezervariIstoric.Count) return "Index invalid.";

            var rez = client.RezervariIstoric[indexRezervare];

            //Ștergem din istoricul clientului
            client.RezervariIstoric.RemoveAt(indexRezervare);

            //Căutăm clasa și ștergem și de acolo
            foreach(var c in _clase)
            {
                var r = c.Rezervari.FirstOrDefault(x => x.Id == rez.Id);
                if (r != null) { c.Rezervari.Remove(r); break; }
            }

            SaveData();
            _logger.LogInformation($"[CANCEL] {client.Username} a anulat {rez.NumeClasa}");
            return "Rezervare anulată.";
        }

        //salvare date
        private void SaveData()
        {
            try
            {
                var opt = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText("db_admins.json", JsonSerializer.Serialize(_admins, opt));
                File.WriteAllText("db_clients.json", JsonSerializer.Serialize(_clients, opt));
                File.WriteAllText("db_sali.json", JsonSerializer.Serialize(_sali, opt));
                File.WriteAllText("db_oferte.json", JsonSerializer.Serialize(_oferte, opt));
                File.WriteAllText("db_clase.json", JsonSerializer.Serialize(_clase, opt));
            }
            catch (Exception ex) { _logger.LogError($"Save Error: {ex.Message}"); }
        }

        private void LoadData()
        {
            List<T> Load<T>(string f) => File.Exists(f) ? JsonSerializer.Deserialize<List<T>>(File.ReadAllText(f)) : new List<T>();
            _admins = Load<Admin>("db_admins.json");
            _clients = Load<Client>("db_clients.json");
            if (!_admins.Any()) _admins.Add(new Admin("admin", "admin"));
            if (!_clients.Any()) _clients.Add(new Client("client", "client"));

            _sali = Load<Sala>("db_sali.json");
            _oferte = Load<OfertaAbonament>("db_oferte.json");
            _clase = Load<ClasaFitness>("db_clase.json");
        }
    }
}