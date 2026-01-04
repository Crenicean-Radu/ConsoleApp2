using System.IO;
using System.Text.Json;
namespace ConsoleApp2;

public class SalvareInFisier
{
    public static void SaveToFile<T>(string fileName, List<T> data)
    {
        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(fileName, json);
    }

    public static List<T> LoadFromFile<T>(string fileName)
    {
        if (!File.Exists(fileName))
            return new List<T>();

        try
        {
            var json = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }
}