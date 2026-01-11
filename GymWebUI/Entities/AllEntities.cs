using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Esențial pentru nume diferite

namespace GymWebUI.Entities
{
    // --- 1. USER & CLIENT & ADMIN ---
    // Asta am stabilit-o deja, dar o rafinăm să bată cu JSON-ul tău
    public abstract record User(string Username, string Password, string Rol);

    public record Admin(string Username, string Password) 
        : User(Username, Password, "Admin");

    public record Client(string Username, string Password) 
        : User(Username, Password, "Client")
    {
        // ATENȚIE: În db_clients.json ai "RezervariIstoric", nu "Rezervari"
        public List<AbonamentClient> Abonamente { get; init; } = new();
        public List<RezervareIstoric> RezervariIstoric { get; init; } = new();
    }

    // --- 2. ABONAMENT (Ce are clientul în buzunar) ---
    // Conform db_clients.json
    public record AbonamentClient
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string NumeOferta { get; init; }
        public decimal Pret { get; init; }
        public DateTime DataStart { get; init; }
        public DateTime DataSfarsit { get; init; }
        
        // Astea sunt critice, apar în JSON-ul tău!
        public Guid SalaId { get; init; } 
        public string NumeSala { get; init; } 
    }

    // --- 3. REZERVARE ISTORIC (Ce vede clientul) ---
    // Conform db_clients.json
    public record RezervareIstoric
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string UsernameClient { get; init; }
        public string NumeClasa { get; init; }
        public DateTime DataRezervare { get; init; }
    }

    // --- 4. CLASA FITNESS ---
    // Conform db_clase.json
    public record FitnessClass
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Nume { get; init; }
        public string Antrenor { get; init; }
        public DateTime Data { get; init; }
        public int Capacitate { get; init; }
        public Guid SalaId { get; init; } // Legătura cu Sala
        
        // Aici JSON-ul zice "Rezervari", deci e ok
        public List<RezervareIstoric> Rezervari { get; init; } = new(); 
    }

    // --- 5. SALA ---
    // Conform db_sali.json
    public record Gym
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Nume { get; init; }
        public string Program { get; init; }
        public List<GymZone> Zone { get; init; } = new();
    }

    public record GymZone(string Nume, int Capacitate);

    // --- 6. OFERTA ---
    // Conform db_oferte.json
    public record SubscriptionOffer
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Nume { get; init; }
        public decimal Pret { get; init; }
        public int ValabilitateZile { get; init; }
        public Guid SalaId { get; init; }
    }
}