namespace ConsoleApp2;

public class Abonament
{
    public string Tip { get; set; }
    public decimal Pret { get; set; }
    public DateTime DataStart { get; set; }
    public DateTime DataSfarsit { get; set; }

    public bool EsteActiv()
    {
        return DateTime.Now >= DataStart && DateTime.Now <= DataSfarsit;
    }
}