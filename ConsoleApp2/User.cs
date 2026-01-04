namespace ConsoleApp2
{
    public abstract class User
    {
        public string Username { get; protected set; }
        public string Password { get; protected set; }

        protected User(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}