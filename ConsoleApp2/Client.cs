using System.Collections.Generic;

namespace ConsoleApp2;

public class Client : User
{
    public List<Abonament> Abonamente { get; set; } = new List<Abonament>();
    public List<Rezervare> Rezervari { get; set; } = new List<Rezervare>();

    public Client(string username, string password) : base(username, password)
    {
    }
}