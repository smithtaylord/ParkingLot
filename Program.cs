namespace ParkingLot

{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Parking Lot Initialized.");

            var parkingGarage = new ParkingGarage(1500);

            var exit = false;

            while (!exit)
            {
                Console.WriteLine("\n \n \n ###############\nParking Garage Menu:");
                Console.WriteLine("1. Dispense a Ticket");
                Console.WriteLine("2. Validate Ticket");
                Console.WriteLine("3. Get Number of Remaining Spots");
                Console.WriteLine("4. Lookup Ticket by License Plate");
                Console.WriteLine("5. Exit");
                Console.WriteLine("#################");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        DispenseTicket(parkingGarage);
                        break;
                    case "2":
                        ValidateTicket(parkingGarage);
                        break;
                    case "3":
                        GetNumberOfRemainingSpots(parkingGarage);
                        break;
                    case "4":
                        LookupTicketByLicencePlate(parkingGarage);
                        break;
                    case "5":
                        exit = true;
                        Console.WriteLine("Exit");
                        break;
                    case "9":
                        Add1000Spots(parkingGarage);
                        break;
                    default:
                        Console.WriteLine("Invalid choice! Please try again.");
                        break;
                }
            }
        }

        private static void DispenseTicket(ParkingGarage parkingGarage)
        {
            Console.WriteLine("\n Select Vehicle Type to Park:");
            Console.WriteLine("1. Car");
            Console.WriteLine("2. Motorcycle \n");

            var isValidInput = false;
            var vehicleType = VehicleType.Car;
            while (!isValidInput)
            {
                var vehicleTypeInput = Console.ReadLine();

                try
                {
                    vehicleType = vehicleTypeInput switch
                                  {
                                      "1" => VehicleType.Car, "2" => VehicleType.Motorcycle, _ => throw new ArgumentException("Invalid choice. Please choose again.")
                                  };

                    isValidInput = true;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            var vehicle = new Vehicle(vehicleType);
            try
            {
                var ticket = parkingGarage.DispenseTicket(vehicle);
                Console.WriteLine($"\n Ticket Dispensed! \n Ticket ID: {ticket.Id} \n Spot Assignment: {ticket.SpotAssignment} \n TimeStamp: {ticket.TimeStamp} \n Vehicle ID: {vehicle.Id} \n Vehicle Type: {vehicle.Type} \n License Plate: {vehicle.LicensePlate} ");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\n Unable to dispense ticket: {ex.Message}");
            }
        }

        private static void ValidateTicket(ParkingGarage parkingGarage)
        {
            Console.WriteLine("\n Enter Ticket ID to Validate:");
            var ticketIdInput = Console.ReadLine();

            if (int.TryParse(ticketIdInput, out var ticketId))
            {
                var response = parkingGarage.ValidateTicket(ticketId);
                Console.WriteLine($"\n {response}");
            }
            else
            {
                Console.WriteLine("Invalid Ticket ID. Please enter a number.");
            }
        }

        private static void GetNumberOfRemainingSpots(ParkingGarage parkingGarage)
        {
            var response = parkingGarage.GetNumberOfRemainingSpots();

            Console.WriteLine($"\n Remaining Parking Spots:");
            Console.WriteLine($"Total Empty Spots: {response.TotalEmptySpots}");
            Console.WriteLine($"Motorcycle Only Spots: {response.MotorcycleOnlySpots}");
        }

        private static void LookupTicketByLicencePlate(ParkingGarage parkingGarage)
        {
            Console.WriteLine("\n Enter Licence Plate to Lookup Ticket:");
            var spotAssignmentInput = Console.ReadLine();

            var ticket = parkingGarage.FindTicketIdByLicencePlate(spotAssignmentInput);
            Console.WriteLine(ticket != null 
                ? $"\n Ticket Found! \n Ticket ID: {ticket.Id} \n Spot Assignment: {ticket.SpotAssignment} \n TimeStamp: {ticket.TimeStamp} \n Vehicle ID: {ticket.Vehicle.Id} \n Vehicle Type: {ticket.Vehicle.Type}" 
                : "No ticket found for the given spot assignment.");
        }

        private static void Add1000Spots(ParkingGarage parkingGarage)
        {
            for (var i = 0; i < 1000; i++)
            {
                var vehicleType = VehicleType.Car;
                var vehicle = new Vehicle(vehicleType);
                parkingGarage.DispenseTicket(vehicle);
            }

            Console.WriteLine("\n Added 1000 Parking Spots.");
        }
    }
}