using System.Text.Json;

namespace ParkingLot.Utilities;

public class NextIDService
{
    public static void SaveNextIdState(int nextId, string filePath)
    {
        File.WriteAllText(filePath, nextId.ToString());
    }
    
    public static int LoadNextIdState(string filePath)
    {
        if (File.Exists(filePath))
        {
            return Convert.ToInt32(File.ReadAllText(filePath));
        }
        return 0; 
    }
}