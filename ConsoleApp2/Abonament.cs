namespace ConsoleApp2;

public class Abonament
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NumeOferta { get; set; }
    public decimal Pret { get; set; }
    public DateTime DataStart { get; set; }
    public DateTime DataSfarsit { get; set; }

    
    public Guid? SalaId { get; set; }
    public string NumeSala { get; set; }

    public bool EsteActiv()
    {
        return DateTime.Now >= DataStart && DateTime.Now <= DataSfarsit;
    }
}