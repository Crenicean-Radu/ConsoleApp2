namespace ConsoleApp2;

public class Sala
{
    public string Nume { get; set; }
    public string Program { get; set; }
    public List<Zona> Zone { get; set; } = new();
}