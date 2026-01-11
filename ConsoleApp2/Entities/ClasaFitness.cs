using System;
using System.Collections.Generic;

namespace ConsoleApp2.Entities
{
    public record ClasaFitness
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Nume { get; init; }
        public string Antrenor { get; init; }
        public DateTime Data { get; init; }
        public int Capacitate { get; init; }
        public Guid SalaId { get; init; }
        public List<Rezervare> Rezervari { get; init; } = new();

        public bool AreLocuri() => Rezervari.Count < Capacitate;
    }
}