namespace ConsoleApp2;

public class Rezervare
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UsernameClient { get; set; }
    public string NumeClasa { get; set; } 
    public DateTime DataRezervare { get; set; }
}