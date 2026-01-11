using System;

namespace ConsoleApp2.Entities
{
    public record AbonamentClient
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string NumeOferta { get; init; }
        public decimal Pret { get; init; }
        public DateTime DataStart { get; init; }
        public DateTime DataSfarsit { get; init; }
        public Guid? SalaId { get; init; }
        public string NumeSala { get; init; }

        public bool EsteActiv() => DateTime.Now >= DataStart && DateTime.Now <= DataSfarsit;
    }
}