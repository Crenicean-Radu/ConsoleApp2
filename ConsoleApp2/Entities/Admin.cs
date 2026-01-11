namespace ConsoleApp2.Entities
{
    public record Admin : User
    {
        // Constructor care trimite "Admin" automat la bază
        public Admin(string username, string password) : base(username, password, "Admin") { }
    }
}