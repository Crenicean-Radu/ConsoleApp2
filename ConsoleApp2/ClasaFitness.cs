namespace ConsoleApp2;

public class ClasaFitness
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nume { get; set; }
    public string Antrenor { get; set; }
    public DateTime Data { get; set; }
    public int Capacitate { get; set; }

   
    public Guid SalaId { get; set; }

    public List<Rezervare> Rezervari { get; set; } = new List<Rezervare>();

    public bool AreLocuri()
    {
        return Rezervari.Count < Capacitate;
    }
}