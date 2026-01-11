using System;

namespace GymWeb.Entities
{
    public record OfertaAbonament
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Nume { get; init; }
        public decimal Pret { get; init; }
        public int ValabilitateZile { get; init; }
        public Guid? SalaId { get; init; } 
    }
}