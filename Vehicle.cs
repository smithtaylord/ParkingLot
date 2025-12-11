using ParkingLot.Utilities;

namespace ParkingLot;

public class Vehicle
{
    public int Id { get; set; }
    public VehicleType Type { get; set; }
    public string LicensePlate { get; set; }

    private const string OUTPUT_DIRECTORY = "Output/Vehicle/";
    private const string NEXT_ID_FILE_PATH = "next_id.txt";
    private const string USED_LICENSE_PLATES_FILE_PATH = "used_license_plates.txt";
    
    private static int _nextId = NextIDService.LoadNextIdState(OUTPUT_DIRECTORY+NEXT_ID_FILE_PATH);
    private static readonly HashSet<string> _usedLicensePlates = new HashSet<string>();

    public Vehicle(VehicleType type)
    {
        // Load used license plates from file
        LoadUsedLicensePlates();
        Id = _nextId;
        Type = type;
        LicensePlate = GenerateUniqueLicensePlate();
        _nextId++;
        NextIDService.SaveNextIdState(_nextId, OUTPUT_DIRECTORY+NEXT_ID_FILE_PATH);

    }
    public Vehicle(int id, VehicleType type, string licensePlate)
    {
        Id = id;
        Type = type;
        LicensePlate = licensePlate;
    }

    private void LoadUsedLicensePlates()
    {
        if (File.Exists(OUTPUT_DIRECTORY + USED_LICENSE_PLATES_FILE_PATH))
        {
            var usedPlatesLines = File.ReadAllLines(OUTPUT_DIRECTORY + USED_LICENSE_PLATES_FILE_PATH);

            foreach (var plate in usedPlatesLines)
            {
                _usedLicensePlates.Add(plate);
            }
        }
        else
        {
            _usedLicensePlates.Clear();
        }
    }
    
    private string GenerateUniqueLicensePlate()
    {
        const string letterChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numberChars = "0123456789";
        var random = new Random();
        string licensePlate;

        do
        {
            var letters = new char[3];
            var numbers = new char[3];

            for (int i = 0; i < 3; i++)
            {
                letters[i] = letterChars[random.Next(letterChars.Length)];
                numbers[i] = numberChars[random.Next(numberChars.Length)];
            }

            licensePlate = new string(letters) + new string(numbers);
        } while (_usedLicensePlates.Contains(licensePlate));
        _usedLicensePlates.Add(licensePlate);
        File.WriteAllLines(OUTPUT_DIRECTORY + USED_LICENSE_PLATES_FILE_PATH, _usedLicensePlates);

        return licensePlate;
    }
}

public enum VehicleType
{
    Car,
    Motorcycle
}