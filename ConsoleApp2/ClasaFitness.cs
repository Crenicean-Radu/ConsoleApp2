namespace ConsoleApp2;

public class ClasaFitness
{
    public string Nume { get; set; }
    public string Antrenor { get; set; }
    public DateTime Data { get; set; }
    public int Capacitate { get; set; }

    public List<Rezervare> Rezervari { get; set; } = new();

    public bool AreLocuri()
    {
        return Rezervari.Count < Capacitate;
    }
}