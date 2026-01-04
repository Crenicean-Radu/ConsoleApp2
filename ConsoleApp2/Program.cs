using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static List<User> users;
        static List<Sala> sali;
        static List<OfertaAbonament> oferte;
        static List<ClasaFitness> clase;

        static void Main()
        {
            // Load date
            users = SalvareInFisier.LoadFromFile<User>("users.json");
            sali = SalvareInFisier.LoadFromFile<Sala>("sali.json");
            oferte = SalvareInFisier.LoadFromFile<OfertaAbonament>("oferte.json");
            clase = SalvareInFisier.LoadFromFile<ClasaFitness>("clase.json");

            if (!users.Any())
            {
                users.Add(new Admin("admin", "admin"));
                users.Add(new Client("client", "client"));
                SaveAll();
            }

            Console.WriteLine("GYM NETWORK");
            Console.Write("User: ");
            string u = Console.ReadLine();
            Console.Write("Parola: ");
            string p = Console.ReadLine();

            var user = Autentificare.Login(u, p, users);

            if (user == null) { Console.WriteLine("Nu e bine log inul"); return; }

            if (user is Admin) MeniuAdmin();
            else MeniuClient((Client)user);
        }

        // ADMIN
        static void MeniuAdmin()
        {
            while (true)
            {
                Console.WriteLine("\n--- ADMIN ---");
                Console.WriteLine("1. Adaugă Sală");
                Console.WriteLine("2. Adaugă Ofertă Abonament (Global sau Local)");
                Console.WriteLine("3. Programează Clasă (Într-o sală)");
                Console.WriteLine("4. Monitorizare");
                Console.WriteLine("0. Ieșire");

                switch (Console.ReadLine())
                {
                    case "1": AdaugaSala(); break;
                    case "2": AdaugaOferta(); break;
                    case "3": AdaugaClasa(); break;
                    case "4": Monitorizare();break;
                    case "0": SaveAll(); return;
                }
            }
        } 
        static void Monitorizare()
{
    Console.WriteLine("\nDASHBOARD GYM");

    
    int totalClienti = 0;
    int abonamenteActive = 0;
    int abonamenteVanduteTotal = 0;
    decimal baniIncasati = 0;

    
    foreach (var u in users)
    {
        if (u is Client c)
        {
            totalClienti++;
            abonamenteVanduteTotal += c.Abonamente.Count;
            
            
            foreach (var ab in c.Abonamente)
            {
                if (ab.EsteActiv()) abonamenteActive++;
                baniIncasati += ab.Pret; 
            }
        }
    }

    Console.WriteLine($"\n[BUSINESS]");
    Console.WriteLine($"Total Clienți înregistrați: {totalClienti}");
    Console.WriteLine($"Abonamente Active (în vigoare): {abonamenteActive}");
    Console.WriteLine($"Total Abonamente Vândute (istoric): {abonamenteVanduteTotal}");
    Console.WriteLine($"Venit Total Încasat: {baniIncasati} RON");

    // 2. INFRASTRUCTURĂ (Săli și Zone)
    Console.WriteLine($"\n[SĂLI ȘI ZONE]");
    Console.WriteLine($"Număr Săli Deschise: {sali.Count}");
    
    // Calculăm totalul zonelor
    int totalZone = 0;
    foreach(var s in sali) totalZone += s.Zone.Count;
    Console.WriteLine($"Număr Total Zone (Cardio/Forță etc): {totalZone}");

    // RAPORT CLASE (Grad de Ocupare) 
    Console.WriteLine($"\n[GRAD DE OCUPARE CLASE]");
    if (clase.Count == 0)
    {
        Console.WriteLine("Nu există clase programate.");
    }
    else
    {
        foreach (var c in clase)
        {
            // Căutăm numele sălii ca să știm unde e clasa
            var salaGasita = sali.FirstOrDefault(s => s.Id == c.SalaId);
            string numeSala = salaGasita != null ? salaGasita.Nume : "Sală Necunoscută";

            // Calculăm procentul
            double procent = 0;
            if (c.Capacitate > 0)
                procent = (double)c.Rezervari.Count / c.Capacitate * 100;

            Console.WriteLine($"- {c.Nume} ({numeSala}) | Antrenor: {c.Antrenor}");
            Console.WriteLine($"  Data: {c.Data:g}");
            
            // Afișăm ceva gen: Ocupare:
            Console.WriteLine($"  Ocupare: {c.Rezervari.Count} din {c.Capacitate} locuri ({procent:F1}%)");
            
            // Dacă e plină, punem un avertisment
            if (c.Rezervari.Count >= c.Capacitate)
                Console.WriteLine("  [!] CLASA ESTE FULL!");
            
            Console.WriteLine("-----------------------------");
        }
    }

    Console.WriteLine("\nApasă Enter să te întorci la meniu...");
    Console.ReadLine();
}
        
        

        static void AdaugaSala()
        {
            Console.WriteLine("\nADAUGĂ SALĂ NOUĂ ");

            // 1. Luăm datele generale ale sălii
            Console.Write("Nume sală: ");
            string nume = Console.ReadLine();

            Console.Write("Program (ex: 08-22): ");
            string program = Console.ReadLine();

            // Creăm obiectul (dar încă nu-l punem în listă)
            var salaNoua = new Sala
            {
                Nume = nume,
                Program = program,
                Zone = new List<Zona>() // Inițializăm lista goală
            };

            // 2. Bucla pentru adăugat zone
            Console.WriteLine("\nAdăugăm zone pentru această sală? (da/nu)");
            if (Console.ReadLine().ToLower() == "da")
            {
                while (true)
                {
                    Console.Write("Nume Zonă (ex: Cardio, Spa) sau scrie 'stop' pentru a termina: ");
                    string numeZona = Console.ReadLine();

                    if (numeZona.ToLower() == "stop") break;

                    Console.Write($"Capacitate pentru {numeZona}: ");
                    if (int.TryParse(Console.ReadLine(), out int capacitate))
                    {
                        // Aici legăm zona direct de sala nouă!
                        salaNoua.Zone.Add(new Zona
                        {
                            Nume = numeZona,
                            Capacitate = capacitate
                        });
                        Console.WriteLine($"-> Zona '{numeZona}' adăugată cu succes.");
                    }
                    else
                    {
                        Console.WriteLine("Te rog introdu un număr valid la capacitate.");
                    }
                }
            }

            // 3. Salvăm totul la final
            sali.Add(salaNoua);
            SaveAll();
            Console.WriteLine($"\nGata! Sala '{salaNoua.Nume}' a fost salvată și are {salaNoua.Zone.Count} zone.");
        }

        static void AdaugaOferta()
        {
            Console.Write("Nume Ofertă: ");
            string nume = Console.ReadLine();
            Console.Write("Preț: ");
            decimal pret = decimal.Parse(Console.ReadLine());
            Console.Write("Zile valabilitate: ");
            int zile = int.Parse(Console.ReadLine());

            Console.WriteLine("Este abonament pentru o sală specifică? (da/nu)");
            Guid? salaIdSelectata = null;

            if (Console.ReadLine().ToLower() == "da")
            {
                if (sali.Count == 0) { Console.WriteLine("Nu ai săli create!"); return; }

                Console.WriteLine("Alege sala:");
                for (int i = 0; i < sali.Count; i++)
                    Console.WriteLine($"{i}. {sali[i].Nume}");

                int idx = int.Parse(Console.ReadLine());
                if (idx >= 0 && idx < sali.Count)
                    salaIdSelectata = sali[idx].Id;
                else
                {
                    Console.WriteLine("Index greșit. Anulat.");
                    return;
                }
            }
            // Dacă salaIdSelectata rămâne null, e abonament GLOBAL

            oferte.Add(new OfertaAbonament
            {
                Nume = nume,
                Pret = pret,
                ValabilitateZile = zile,
                SalaId = salaIdSelectata 
            });
            SaveAll();
            Console.WriteLine("Ofertă salvată!");
        }

        static void AdaugaClasa()
        {
            if (sali.Count == 0) { Console.WriteLine("Nu ai săli!"); return; }

            Console.WriteLine("În ce sală se ține clasa?");
            for (int i = 0; i < sali.Count; i++)
                Console.WriteLine($"{i}. {sali[i].Nume}");

            int idx = int.Parse(Console.ReadLine());
            if (idx < 0 || idx >= sali.Count) return;
            var salaAleasa = sali[idx];

            Console.Write("Nume Clasă: ");
            string nume = Console.ReadLine();
            Console.Write("Antrenor: ");
            string antrenor = Console.ReadLine();
            Console.Write("Capacitate: ");
            int cap = int.Parse(Console.ReadLine());

            clase.Add(new ClasaFitness
            {
                Nume = nume,
                Antrenor = antrenor,
                Capacitate = cap,
                Data = DateTime.Now.AddDays(1),
                SalaId = salaAleasa.Id // Legăm clasa de sală
            });
            SaveAll();
            Console.WriteLine($"Clasa adăugată la {salaAleasa.Nume}!");
        }

        // CLIENT
        static void MeniuClient(Client client)
        {
            while (true)
            {
                Console.WriteLine("\n--- CLIENT ---");
                Console.WriteLine("1. Cumpără Abonament");
                Console.WriteLine("2. Rezervă Clasă (Doar unde ai acces)");
                Console.WriteLine("3. Abonamentele Mele");
                Console.WriteLine("0. Exit");

                switch (Console.ReadLine())
                {
                    case "1": CumparaAbonament(client); break;
                    case "2": RezervaClasa(client); break;
                    case "3": VeziAbonamente(client); break;
                    case "0": SaveAll(); return;
                }
            }
        }

        static void CumparaAbonament(Client client)
        {
            if (oferte.Count == 0) { Console.WriteLine("Nu sunt oferte."); return; }

            Console.WriteLine("Oferte:");
            for (int i = 0; i < oferte.Count; i++)
            {
                var of = oferte[i];
                string loc = of.SalaId == null ? "GLOBAL (Toate sălile)" : "Doar " + sali.FirstOrDefault(s => s.Id == of.SalaId)?.Nume;
                Console.WriteLine($"{i}. {of.Nume} - {of.Pret} RON - {loc}");
            }

            Console.Write("Alege: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 0 && idx < oferte.Count)
            {
                var of = oferte[idx];

                string numeSala = "Global";
                if(of.SalaId != null)
                    numeSala = sali.FirstOrDefault(s => s.Id == of.SalaId)?.Nume ?? "Necunoscută";

                client.Abonamente.Add(new Abonament
                {
                    NumeOferta = of.Nume,
                    Pret = of.Pret,
                    DataStart = DateTime.Now,
                    DataSfarsit = DateTime.Now.AddDays(of.ValabilitateZile),
                    SalaId = of.SalaId, // Important!
                    NumeSala = numeSala
                });
                SaveAll();
                Console.WriteLine("Cumpărat!");
            }
        }

        static void RezervaClasa(Client client)
        {
            Console.WriteLine("Clase disponibile:");
            for (int i = 0; i < clase.Count; i++)
            {
                var c = clase[i];
                var numeSala = sali.FirstOrDefault(s => s.Id == c.SalaId)?.Nume ?? "???";
                Console.WriteLine($"{i}. {c.Nume} ({numeSala}) - {c.Data:g}");
            }

            Console.Write("Alege clasa: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 0 && idx < clase.Count)
            {
                var clasaAleasa = clase[idx];

                
                // Căutăm dacă clientul are vreun abonament ACTIV care să acopere sala asta
                bool areAcces = false;

                foreach(var ab in client.Abonamente)
                {
                    if (ab.EsteActiv())
                    {
                        // 1. Dacă abonamentul e Global (SalaId == null) -> Are acces oriunde
                        if (ab.SalaId == null)
                        {
                            areAcces = true;
                            break;
                        }
                        // 2. Dacă abonamentul e pe sala curentă
                        if (ab.SalaId == clasaAleasa.SalaId)
                        {
                            areAcces = true;
                            break;
                        }
                    }
                }

                if (!areAcces)
                {
                    Console.WriteLine("EROARE: Nu ai abonament activ pentru această sală!");
                    return;
                }

                if (clasaAleasa.AreLocuri())
                {
                    var rez = new Rezervare
                    {
                        UsernameClient = client.Username,
                        NumeClasa = clasaAleasa.Nume,
                        DataRezervare = DateTime.Now
                    };
                    clasaAleasa.Rezervari.Add(rez);
                    client.Rezervari.Add(rez);
                    SaveAll();
                    Console.WriteLine("Rezervare reușită!");
                }
                else Console.WriteLine("Nu mai sunt locuri.");
            }
        }

        static void VeziAbonamente(Client c)
        {
            foreach(var a in c.Abonamente)
                Console.WriteLine($"{a.NumeOferta} ({a.NumeSala}) - Activ: {a.EsteActiv()}");
        }

        static void SaveAll()
        {
            SalvareInFisier.SaveToFile("users.json", users);
            SalvareInFisier.SaveToFile("sali.json", sali);
            SalvareInFisier.SaveToFile("oferte.json", oferte);
            SalvareInFisier.SaveToFile("clase.json", clase);
        }
    }
}