using System.Diagnostics;

namespace ParkingLot;

public class ParkingGarage
{
    public int Capacity { get; set; }
    private HashSet<string> FullyOpenSpots { get; set; }
    private HashSet<string> SpotsWithOneMotorcycle { get; set; }

    private const string OUTPUT_DIRECTORY = "Output/ParkingGarage/";
    private const string FULLY_OPEN_SPOTS_FILE_NAME = "fully_open_spots.txt";
    private const string SPOTS_WITH_ONE_MOTORCYCLE_FILE_NAME = "spots_with_one_motorcycle.txt";
    private const string TICKET_DICTIONARY_FILE_NAME = "ticket_dictionary_table.txt";
    private const string LICENCE_PLATE_INDEX_TABLE_FILE_NAME = "licence_plate_index_table.txt";

    private readonly List<string> _ticketTable = new();
    private List<string> _licencePlateIndexTable = new();

    public ParkingGarage(int capacity)
    {
        Capacity = capacity;
        FullyOpenSpots = LoadStoredHashSet(OUTPUT_DIRECTORY + FULLY_OPEN_SPOTS_FILE_NAME);
        SpotsWithOneMotorcycle = LoadStoredHashSet(OUTPUT_DIRECTORY + SPOTS_WITH_ONE_MOTORCYCLE_FILE_NAME);

        if (FullyOpenSpots.Count == 0 && SpotsWithOneMotorcycle.Count == 0)
        {
            InitializeParkingSpots(capacity);
        }

        LoadTicketDictionary();
        LoadLicencePlateIndexTable();
    }


    private void InitializeParkingSpots(int capacity)
    {
        for (var i = 1; i <= capacity; i++)
        {
            FullyOpenSpots.Add($"{i}");
        }
    }

    public Ticket DispenseTicket(Vehicle vehicle)
    {
        string? parkingSpot = null;

        if (vehicle.Type == VehicleType.Car && FullyOpenSpots.Count > 0)
        {
            parkingSpot = FullyOpenSpots.First();
            FullyOpenSpots.Remove(parkingSpot);
        }
        else if (vehicle.Type == VehicleType.Motorcycle)
        {
            if (SpotsWithOneMotorcycle.Count > 0)
            {
                parkingSpot = SpotsWithOneMotorcycle.First();
                SpotsWithOneMotorcycle.Remove(parkingSpot);
            }
            else if (FullyOpenSpots.Count > 0)
            {
                parkingSpot = FullyOpenSpots.First();
                FullyOpenSpots.Remove(parkingSpot);
                SpotsWithOneMotorcycle.Add(parkingSpot);
            }
        }

        if (parkingSpot != null)
        {
            var ticket = new Ticket(parkingSpot, vehicle);

            AddTicketToDictionaryLines(ticket);
            SaveGarageState();
            return ticket;
        }
        else
        {
            throw new InvalidOperationException("Parking Lot Full.");
        }
    }


    public string ValidateTicket(int ticketId)
    {
        var ticket = FindTicket(ticketId);
        if (ticket == null)
        {
            return "Invalid Ticket.";
        }

        var parkingSpot = ticket.SpotAssignment;
        if (ticket.Vehicle.Type == VehicleType.Car)
        {
            FullyOpenSpots.Add(parkingSpot);
        }
        else if (ticket.Vehicle.Type == VehicleType.Motorcycle)
        {
            if (SpotsWithOneMotorcycle.Contains(parkingSpot))
            {
                SpotsWithOneMotorcycle.Remove(parkingSpot);
                FullyOpenSpots.Add(parkingSpot);
            }
            else
            {
                SpotsWithOneMotorcycle.Add(parkingSpot);
            }
        }

        RemoveTicketFromDictionaryLines(ticketId);
        SaveGarageState();
        return $"Ticket Validated. Retrieve vehicle from spot {parkingSpot}.";
    }


    public NumberOfRemainingSpots GetNumberOfRemainingSpots()
    {
        return new NumberOfRemainingSpots()
        {
            TotalEmptySpots = FullyOpenSpots.Count, MotorcycleOnlySpots = SpotsWithOneMotorcycle.Count,
        };
    }

    public Ticket? FindTicketIdByLicencePlate(string licencePlate)
    {
        var index = FindIndexByLicencePlateBinarySearch(licencePlate);
        Console.WriteLine($"index found: {index}");
        
        if (index == -1) return null;
        var ticketTableIndex = int.Parse(_licencePlateIndexTable[index].Split(",")[1].Trim());
        var ticketTableParts = _ticketTable[ticketTableIndex + 1].Split(",");
        Console.WriteLine($"VehicleID found: {ticketTableParts[3]}");

        var vehicle = new Vehicle(int.Parse(ticketTableParts[3]), Enum.Parse<VehicleType>(ticketTableParts[4]), ticketTableParts[5]);
        return new Ticket(
            int.Parse(ticketTableParts[0]),
            DateTime.Parse(ticketTableParts[1]),
            ticketTableParts[2],
            vehicle);
    }






    private void AddTicketToDictionaryLines(Ticket ticket)
    {
        var line = $"{ticket.Id},{ticket.TimeStamp},{ticket.SpotAssignment},{ticket.Vehicle.Id},{ticket.Vehicle.Type},{ticket.Vehicle.LicensePlate}";
        _ticketTable.Add(line);

        AddLicencePlateToIndex(ticket.Vehicle.LicensePlate, _ticketTable.Count - 2); // -1 to account for header
    }
    
    private void RemoveTicketFromDictionaryLines(int ticketId)
    {
        var ticketIndex = FindIndexBinarySearch(ticketId, _ticketTable);
        if (ticketIndex != -1)
        {
            var licencePlate = _ticketTable[ticketIndex].Split(',')[5].Trim();
            _ticketTable.RemoveAt(ticketIndex);
        
            // Remove from index table
            RemoveLicencePlateFromIndex(licencePlate);
        
            // Update all ticket indices in the index table
            UpdateIndicesAfterRemoval(ticketIndex);
        }
        else
        {
            Console.WriteLine($"Warning: Ticket ID {ticketId} not found in the list.");
        }
    }
    
    private void AddLicencePlateToIndex(string licencePlate, int ticketTableIndex)
    {
        var insertPosition = BinarySearchInsertPositionLicencePlate(licencePlate) + 1;
        _licencePlateIndexTable.Insert(insertPosition, $"{licencePlate},{ticketTableIndex}");
    }


    
    private void RemoveLicencePlateFromIndex(string licencePlate)
    {
        for (int i = 1; i < _licencePlateIndexTable.Count; i++)
        {
            var parts = _licencePlateIndexTable[i].Split(',');
            var currentSpot = parts[0];
        
            if (currentSpot == licencePlate)
            {
                _licencePlateIndexTable.RemoveAt(i);
                return;
            }
        }
    }

    private void UpdateIndicesAfterRemoval(int removedTicketIndex)
    {
        for (int i = 1; i < _licencePlateIndexTable.Count; i++)
        {
            var parts = _licencePlateIndexTable[i].Split(',');
            var parkingSpot = parts[0];
            var ticketIndex = int.Parse(parts[1]);
        
            if (ticketIndex > removedTicketIndex -1)
            {
                _licencePlateIndexTable[i] = $"{parkingSpot},{ticketIndex - 1}";
            }
        }
    }


    private Ticket? FindTicket(int ticketId)
    {
        var index = FindIndexBinarySearch(ticketId, _ticketTable);
        if (index == -1) return null;

        var line = _ticketTable[index];
        var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5)
            return null;

        var timeStamp = DateTime.Parse(parts[1]);
        var spotAssignment = parts[2];
        var vehicleId = int.Parse(parts[3]);
        var vehicleType = Enum.Parse<VehicleType>(parts[4]);

        var vehicle = new Vehicle(vehicleType) { Id = vehicleId };
        var ticket = new Ticket(spotAssignment, vehicle) { Id = ticketId, TimeStamp = timeStamp };

        return ticket;
    }


    
    // ************
    // Binary Search Helpers
    // ************
    private int FindIndexBinarySearch(int input, List<string> table)
    {
        var left = 1; // Skip header
        var right = table.Count - 1;

        while (left <= right)
        {
            var midPoint = left + (right - left) / 2;
            var line = table[midPoint];
            var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!int.TryParse(parts[0], out var id)) continue;

            if (id == input) return midPoint;
            if (id < input)
            {
                left = midPoint + 1;
            }
            else
            {
                right = midPoint - 1;
            }
        }

        return -1;
    }
    private int FindIndexByLicencePlateBinarySearch(string licencePlate)
    {
        var left = 1; // Skip header
        var right = _licencePlateIndexTable.Count - 1;

        while (left <= right)
        {
            var midPoint = left + (right - left) / 2;
            var line = _licencePlateIndexTable[midPoint];
            var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var plate = parts[0];

            if (plate == licencePlate) return midPoint;
            if (string.Compare(plate, licencePlate, StringComparison.Ordinal) < 0)
            {
                left = midPoint + 1;
            }
            else
            {
                right = midPoint - 1;
            }
        }

        return -1;
    }
    
    private int BinarySearchInsertPositionLicencePlate(string licencePlate)
    {
        var low = 0;
        var high = _licencePlateIndexTable.Count - 1;
        
        while (low <= high)
        {
            var mid = low + (high - low) / 2;
        
            if (mid == 0)
            {
                low = 1;
                continue;
            }
        
            var midSpot = _licencePlateIndexTable[mid].Split(',')[0];
            var compareResult = string.Compare(midSpot, licencePlate, StringComparison.Ordinal);
        
            if (compareResult == 0)
            {
                return mid;
            }
            else if (compareResult < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        
        return low - 1;
    }
    
    // ************
    // SAVING AND LOADING GARAGE STATE
    // ************

    private void SaveGarageState()
    {
        File.WriteAllLines(OUTPUT_DIRECTORY + FULLY_OPEN_SPOTS_FILE_NAME, FullyOpenSpots);
        File.WriteAllLines(OUTPUT_DIRECTORY + SPOTS_WITH_ONE_MOTORCYCLE_FILE_NAME, SpotsWithOneMotorcycle);
        File.WriteAllLines(OUTPUT_DIRECTORY + TICKET_DICTIONARY_FILE_NAME, _ticketTable);
        File.WriteAllLines(OUTPUT_DIRECTORY + LICENCE_PLATE_INDEX_TABLE_FILE_NAME, _licencePlateIndexTable);
    }
    
    private void LoadTicketDictionary()
    {
        if (File.Exists(OUTPUT_DIRECTORY + TICKET_DICTIONARY_FILE_NAME))
        {
            var ticketTableLines = File.ReadAllLines(OUTPUT_DIRECTORY + TICKET_DICTIONARY_FILE_NAME);

            _ticketTable.AddRange(ticketTableLines);
        }
        else
        {
            _ticketTable.Clear();
            _ticketTable.Add("TicketId,TimeStamp,SpotAssignment,VehicleId,VehicleType,VehicleLicensePlate");
        }
    }

    private void LoadLicencePlateIndexTable()
    {
        if (File.Exists(OUTPUT_DIRECTORY + LICENCE_PLATE_INDEX_TABLE_FILE_NAME))
        {
            var indexTableLines = File.ReadAllLines(OUTPUT_DIRECTORY + LICENCE_PLATE_INDEX_TABLE_FILE_NAME);
            _licencePlateIndexTable = indexTableLines.ToList();
        }
        else
        {
            _licencePlateIndexTable.Clear();
            const string header = "licence_plate,ticket_dict_table_index";
            _licencePlateIndexTable = new List<string> { header };
        }
    }
    
    private HashSet<string> LoadStoredHashSet(string filePath)
    {
        var watch = Stopwatch.StartNew();

        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            watch.Stop();
            Console.WriteLine($"Reading file {Path.GetFileName(filePath)} took {watch.ElapsedMilliseconds}ms");

            watch.Restart();
            var result = new HashSet<string>(lines);
            watch.Stop();
            Console.WriteLine($"Deserializing {Path.GetFileName(filePath)} took {watch.ElapsedMilliseconds}ms");
            return result;
        }

        watch.Stop();
        return new HashSet<string>();
    }
}

public class NumberOfRemainingSpots
{
    public int TotalEmptySpots { get; set; }
    public int MotorcycleOnlySpots { get; set; }
}