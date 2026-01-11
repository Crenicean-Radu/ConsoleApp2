using System;

namespace ConsoleApp2.Entities
{
    public record Rezervare
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string UsernameClient { get; init; }
        public string NumeClasa { get; init; }
        public DateTime DataRezervare { get; init; }
    }
}