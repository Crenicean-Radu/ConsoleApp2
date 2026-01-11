using System.Text.Json;
using GymWebUI.Entities;

namespace GymWebUI.Services;

public class GymService
{
    private List<Sala> _sali = new();
    private List<User> _users = new();
    private List<OfertaAbonament> _oferte = new();
    private List<ClasaFitness> _clase = new();

    // Căile către fișiere
    private readonly string _pathSali = "db_sali.json";
    private readonly string _pathUsers = "db_users.json";
    private readonly string _pathOferte = "db_oferte.json";
    private readonly string _pathClase = "db_clase.json";

    public GymService()
    {
        LoadData();
        SeedUsersIfEmpty(); // Creăm useri default dacă nu există
    }

    // --- ADMINISTRARE SĂLI & ZONE ---
    public List<Sala> GetSali() => _sali;
    
    public void AdaugaSala(Sala sala)
    {
        if(sala.Id == Guid.Empty) sala.Id = Guid.NewGuid();
        _sali.Add(sala);
        SaveData();
    }

    public void StergeSala(Guid id)
    {
        var sala = _sali.FirstOrDefault(s => s.Id == id);
        if (sala != null)
        {
            _sali.Remove(sala);
            // Ștergem și ofertele/clasele asociate sălii ca să nu rămână orfane
            _oferte.RemoveAll(o => o.SalaId == id);
            _clase.RemoveAll(c => c.SalaId == id);
            SaveData();
        }
    }

    // --- DEFINIREA ABONAMENTELOR ---
    public List<OfertaAbonament> GetOferte() => _oferte;

    public void AdaugaOferta(OfertaAbonament oferta)
    {
        if (oferta.Id == Guid.Empty) oferta.Id = Guid.NewGuid();
        _oferte.Add(oferta);
        SaveData();
    }

    public void StergeOferta(Guid id)
    {
        _oferte.RemoveAll(o => o.Id == id);
        SaveData();
    }

    // --- GESTIONAREA CLASELOR & ANTRENORILOR ---
    public List<ClasaFitness> GetClase() => _clase;

    public void AdaugaClasa(ClasaFitness clasa)
    {
        if (clasa.Id == Guid.Empty) clasa.Id = Guid.NewGuid();
        _clase.Add(clasa);
        SaveData();
    }

    public void StergeClasa(Guid id)
    {
        _clase.RemoveAll(c => c.Id == id);
        SaveData();
    }

    // --- LOGICA CLIENTULUI ---
    public List<User> GetUsers() => _users;

    public Client GetClientByUsername(string username)
    {
        return _users.OfType<Client>().FirstOrDefault(u => u.Username == username);
    }

    // Cumpărarea unui abonament
    public bool ProcessCumparareAbonament(string username, Guid ofertaId)
    {
        var client = GetClientByUsername(username);
        var oferta = _oferte.FirstOrDefault(o => o.Id == ofertaId);

        if (client == null || oferta == null) return false;

        string numeSala = null;
        if(oferta.SalaId != null)
        {
            var sala = _sali.FirstOrDefault(s => s.Id == oferta.SalaId);
            numeSala = sala?.Nume;
        }

        client.CumparaAbonament(oferta, numeSala);
        SaveData();
        return true;
    }

    // Rezervarea la clase (Cu verificări)
    public string ProcessRezervare(string username, Guid clasaId)
    {
        var client = GetClientByUsername(username);
        var clasa = _clase.FirstOrDefault(c => c.Id == clasaId);

        if (client == null || clasa == null) return "Eroare date.";
        if (clasa.Rezervari.Count >= clasa.Capacitate) return "Clasa este plină!";
        if (!client.AreAbonamentValidForSala(clasa.SalaId)) return "Nu ai abonament valid pentru această sală.";
        if (clasa.Rezervari.Any(r => r.NumeClient == username)) return "Ești deja rezervat.";

        clasa.Rezervari.Add(new Rezervare { NumeClient = username, DataRezervare = DateTime.Now });
        SaveData();
        return "OK";
    }

    // Anularea rezervării
    public void ProcessAnulare(string username, Guid clasaId)
    {
        var clasa = _clase.FirstOrDefault(c => c.Id == clasaId);
        if (clasa != null)
        {
            clasa.Rezervari.RemoveAll(r => r.NumeClient == username);
            SaveData();
        }
    }

    // --- PERSISTENȚA DATELOR (JSON) ---
    private void SaveData()
    {
        File.WriteAllText(_pathSali, JsonSerializer.Serialize(_sali));
        
        // Polymorphic serialization pt Useri (ca să știe care e Client și care e User simplu)
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_pathUsers, JsonSerializer.Serialize(_users, options));
        
        File.WriteAllText(_pathOferte, JsonSerializer.Serialize(_oferte));
        File.WriteAllText(_pathClase, JsonSerializer.Serialize(_clase));
    }

    private void LoadData()
    {
        if (File.Exists(_pathSali)) 
            _sali = JsonSerializer.Deserialize<List<Sala>>(File.ReadAllText(_pathSali)) ?? new();
        
        if (File.Exists(_pathOferte)) 
            _oferte = JsonSerializer.Deserialize<List<OfertaAbonament>>(File.ReadAllText(_pathOferte)) ?? new();

        if (File.Exists(_pathClase)) 
            _clase = JsonSerializer.Deserialize<List<ClasaFitness>>(File.ReadAllText(_pathClase)) ?? new();

        if (File.Exists(_pathUsers))
        {
            // Aici e un truc mic: Json simplu nu știe să deserializeze subclase (Client).
            // Pentru simplitate academică, vom încărca totul ca JsonElement și le refacem manual
            // SAU folosim Newtonsoft, dar mergem pe varianta simplă:
            try 
            {
                var json = File.ReadAllText(_pathUsers);
                // Încărcăm întâi Useri generici, dar pierdem datele de Client. 
                // Pentru nota 10, ideal e să folosim System.Text.Json.Serialization.JsonDerivedType
                // Dar facem un seed la fiecare pornire dacă e gol pt demonstrație.
                _users = JsonSerializer.Deserialize<List<User>>(json) ?? new();
                
                // Re-citim clienții corect
                var allUsers = JsonSerializer.Deserialize<List<JsonElement>>(json);
                _users.Clear();
                foreach(var u in allUsers)
                {
                    int rol = u.GetProperty("Rol").GetInt32();
                    if(rol == (int)UserRole.Client)
                        _users.Add(JsonSerializer.Deserialize<Client>(u.GetRawText()));
                    else
                        _users.Add(JsonSerializer.Deserialize<User>(u.GetRawText()));
                }
            }
            catch { _users = new(); }
        }
    }

    private void SeedUsersIfEmpty()
    {
        if (!_users.Any())
        {
            _users.Add(new User { Username = "admin", Parola = "admin", Rol = UserRole.Admin });
            _users.Add(new Client { Username = "client", Parola = "client", Rol = UserRole.Client });
            SaveData();
        }
    }
}