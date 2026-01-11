using System;
using System.Collections.Generic;

namespace ConsoleApp2.Entities
{
    public record Sala
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Nume { get; init; }
        public string Program { get; init; }
        public List<Zona> Zone { get; init; } = new();
    }
}