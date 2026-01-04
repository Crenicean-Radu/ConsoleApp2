using System.Collections.Generic;

namespace ConsoleApp2;

public class Client : User
{
    public List<Abonament> Abonamente { get; set; } = new();
    public List<Rezervare> Rezervari { get; set; } = new();

    public Client(string username, string password) : base(username, password)
    {
    }
}