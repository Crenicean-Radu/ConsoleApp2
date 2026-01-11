using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ConsoleApp2.Services;
using ConsoleApp2.Entities;

namespace ConsoleApp2
{
    public class AppWorker : BackgroundService
    {
        private readonly GymService _service;
        private readonly IHostApplicationLifetime _lifetime;

        public AppWorker(GymService service, IHostApplicationLifetime lifetime)
        {
            _service = service;
            _lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(500, stoppingToken);
            Console.WriteLine("\n=== GYM SYSTEM v3.0 (FULL LOGIC) ===");
            
            Console.Write("User: "); string u = Console.ReadLine();
            Console.Write("Pass: "); string p = Console.ReadLine();

            if (!_service.Login(u, p))
            {
                Console.WriteLine("Login eșuat.");
                _lifetime.StopApplication();
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var user = _service.CurrentUser;
                Console.WriteLine($"\n--- Logat ca {user.Username} ---");
                if (user is Admin) AdminMenu();
                else if (user is Client) ClientMenu();
                else break;
            }
        }

        //MENIU ADMIN
        void AdminMenu()
        {
            Console.WriteLine("1. Săli (Adaugă/Șterge/Modifică)");
            Console.WriteLine("2. Oferte Abonament");
            Console.WriteLine("3. Clase Fitness");
            Console.WriteLine("4. Monitorizare");
            Console.WriteLine("0. Exit");
            
            switch(Console.ReadLine())
            {
                case "1": SubSali(); break;
                case "2": SubOferte(); break;
                case "3": SubClase(); break;
                case "4": _service.PrintMonitorizare(); break;
                case "0": _lifetime.StopApplication(); Environment.Exit(0); break;
            }
        }

        void SubSali()
        {
            var sali = _service.GetSali();
            Console.WriteLine("\n[LISTA SĂLI]");
            for(int i=0; i<sali.Count; i++) Console.WriteLine($"{i}. {sali[i].Nume} ({sali[i].Program})");
            
            Console.WriteLine("A = Adaugă | S = Șterge | M = Modifică | X = Back");
            var cmd = Console.ReadLine().ToUpper();
            
            if (cmd == "A")
            {
                Console.Write("Nume: "); string n = Console.ReadLine();
                Console.Write("Program: "); string p = Console.ReadLine();
                var zone = new List<Zona>(); 
                _service.AdaugaSala(n, p, zone);
            }
            else if (cmd == "S")
            {
                Console.Write("Index: ");
                if(int.TryParse(Console.ReadLine(), out int idx)) _service.StergeSala(idx);
            }
            else if (cmd == "M")
            {
                Console.Write("Index: ");
                if(int.TryParse(Console.ReadLine(), out int idx))
                {
                    Console.Write("Nume Nou: "); string n = Console.ReadLine();
                    Console.Write("Prog Nou: "); string p = Console.ReadLine();
                    _service.ModificaSala(idx, n, p);
                }
            }
        }

        void SubOferte()
        {
            var of = _service.GetOferte();
            Console.WriteLine("\n[LISTA OFERTE]");
            for(int i=0; i<of.Count; i++) 
            {
                string loc = of[i].SalaId == null ? "GLOBAL" : "Local";
                Console.WriteLine($"{i}. {of[i].Nume} - {of[i].Pret} RON ({loc})");
            }
            
            Console.WriteLine("A = Adaugă | S = Sterge | X = Back");
            if(Console.ReadLine().ToUpper() == "A")
            {
                Console.Write("Nume: "); string n = Console.ReadLine();
                Console.Write("Preț: "); decimal p = decimal.Parse(Console.ReadLine());
                Console.Write("Zile: "); int z = int.Parse(Console.ReadLine());
                Guid? sId = null;
                Console.WriteLine("Legi oferta de o sală? (da/nu)");
                if(Console.ReadLine() == "da")
                {
                    var s = _service.GetSali();
                    for(int i=0; i<s.Count; i++) Console.WriteLine($"{i}. {s[i].Nume}");
                    int idx = int.Parse(Console.ReadLine());
                    if(idx>=0 && idx<s.Count) sId = s[idx].Id;
                }
                _service.AdaugaOferta(n, p, z, sId);
            }
            else if(Console.ReadLine().ToUpper() == "S")
            {
                 Console.Write("Index: ");
                 if(int.TryParse(Console.ReadLine(), out int idx)) _service.StergeOferta(idx);
            }
        }

        void SubClase()
        {
            var cl = _service.GetClase();
            Console.WriteLine("\n[LISTA CLASE]");
            for(int i=0; i<cl.Count; i++) Console.WriteLine($"{i}. {cl[i].Nume} ({cl[i].Antrenor})");
            
            Console.WriteLine("A = Adaugă | S = Șterge | M = Modifică | X = Back");
            var cmd = Console.ReadLine().ToUpper();
            
            if(cmd == "A")
            {
                var s = _service.GetSali();
                if(!s.Any()) { Console.WriteLine("Nu ai săli!"); return; }
                for(int i=0; i<s.Count; i++) Console.WriteLine($"{i}. {s[i].Nume}");
                Console.Write("Alege sala index: "); int si = int.Parse(Console.ReadLine());
                
                Console.Write("Nume: "); string n = Console.ReadLine();
                Console.Write("Antrenor: "); string a = Console.ReadLine();
                Console.Write("Capacitate: "); int c = int.Parse(Console.ReadLine());
                _service.ProgrameazaClasa(n, a, c, s[si].Id);
            }
            else if(cmd == "S")
            {
                Console.Write("Index: ");
                if(int.TryParse(Console.ReadLine(), out int idx)) _service.StergeClasa(idx);
            }
            else if(cmd == "M")
            {
                Console.Write("Index: ");
                if(int.TryParse(Console.ReadLine(), out int idx))
                {
                    Console.Write("Nume: "); string n = Console.ReadLine();
                    Console.Write("Antrenor: "); string a = Console.ReadLine();
                    Console.Write("Cap: "); int c = int.Parse(Console.ReadLine());
                    _service.ModificaClasa(idx, n, a, c);
                }
            }
        }

        // MENIU CLIENT
        void ClientMenu()
        {
            Console.WriteLine("1. Cumpără Abonament");
            Console.WriteLine("2. Rezervă Clasă");
            Console.WriteLine("3. Profilul Meu (Abonamente & Istoric)");
            Console.WriteLine("4. Anulează Rezervare");
            Console.WriteLine("0. Exit");
            
            switch(Console.ReadLine())
            {
                case "1": CumparaFlow(); break;
                case "2": RezervaFlow(); break;
                case "3": ProfilFlow(); break;
                case "4": AnuleazaFlow(); break;
                case "0": _lifetime.StopApplication(); Environment.Exit(0); break;
            }
        }

        void CumparaFlow()
        {
            var of = _service.GetOferte();
            Console.WriteLine("\n--- OFERTE ---");
            for(int i=0; i<of.Count; i++) 
            {
                string t = of[i].SalaId == null ? "Standard (Alegi tu sala)" : "Promo Local";
                Console.WriteLine($"{i}. {of[i].Nume} - {of[i].Pret} RON ({t})");
            }
            
            Console.Write("Alege Oferta Index: ");
            if(int.TryParse(Console.ReadLine(), out int idx))
            {
                int salaIdx = -1;
                // Dacă oferta e generică, cerem sala
                if(idx >=0 && idx < of.Count && of[idx].SalaId == null)
                {
                    Console.WriteLine("Pentru ce sală dorești activarea?");
                    var s = _service.GetSali();
                    for(int i=0; i<s.Count; i++) Console.WriteLine($"{i}. {s[i].Nume}");
                    Console.Write("Alege sala index: ");
                    int.TryParse(Console.ReadLine(), out salaIdx);
                }
                Console.WriteLine(_service.CumparaAbonament(idx, salaIdx));
            }
        }

        void RezervaFlow()
        {
            var cl = _service.GetClase();
            var s = _service.GetSali();
            Console.WriteLine("\n--- CLASE ---");
            for(int i=0; i<cl.Count; i++)
            {
                string sn = s.FirstOrDefault(x=>x.Id == cl[i].SalaId)?.Nume ?? "?";
                Console.WriteLine($"{i}. {cl[i].Nume} la {sn} ({cl[i].Rezervari.Count}/{cl[i].Capacitate})");
            }
            Console.Write("Index Clasă: ");
            if(int.TryParse(Console.ReadLine(), out int idx))
                Console.WriteLine(_service.RezervaClasa(idx));
        }

        void ProfilFlow()
        {
            var c = _service.GetCurrentClient();
            Console.WriteLine("\n[ABONAMENTE]");
            foreach(var a in c.Abonamente) 
                Console.WriteLine($"- {a.NumeOferta} la {a.NumeSala} | Activ: {a.EsteActiv()}");

            Console.WriteLine("\n[ISTORIC REZERVĂRI]");
            for(int i=0; i<c.RezervariIstoric.Count; i++)
            {
                var r = c.RezervariIstoric[i];
                Console.WriteLine($"{i}. {r.NumeClasa} ({r.DataRezervare:g})");
            }
        }

        void AnuleazaFlow()
        {
            ProfilFlow(); // Arată lista ca să știe ce index să folosim
            Console.Write("Introdu indexul rezervării de anulat: ");
            if(int.TryParse(Console.ReadLine(), out int idx))
                Console.WriteLine(_service.AnuleazaRezervare(idx));
        }
    }
}