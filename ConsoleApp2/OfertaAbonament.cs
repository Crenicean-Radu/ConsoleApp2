namespace ConsoleApp2;

public class OfertaAbonament
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nume { get; set; }
    public decimal Pret { get; set; }
    public int ValabilitateZile { get; set; }

  
    public Guid? SalaId { get; set; }
}