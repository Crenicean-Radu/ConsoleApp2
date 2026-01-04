namespace ConsoleApp2;

public class Autentificare
{
    public static User Login(string username, string password, List<User> users)
    {
        return users.FirstOrDefault(u => u.Username == username && u.Password == password);
    }
}