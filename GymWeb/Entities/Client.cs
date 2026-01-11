using System.Collections.Generic;

namespace GymWeb.Entities
{
    public record Client : User
    {
        public List<AbonamentClient> Abonamente { get; init; } = new();
        public List<Rezervare> RezervariIstoric { get; init; } = new();

        public Client(string username, string password) : base(username, password, "Client") { }
    }
}