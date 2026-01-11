using System;

namespace GymWeb.Entities
{
    public record Rezervare
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string UsernameClient { get; init; }
        public string NumeClasa { get; init; }
        public DateTime DataRezervare { get; init; }
    }
}