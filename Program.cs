using System.IO.Pipes;

namespace ParkingLot

{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Parking Lot Server Started.");
            Console.WriteLine("Parking Lot Initialized.");

            var parkingGarage = new ParkingGarage(1500);


            // Start listening for ticket machine connections
            await ListenForTicketMachines(parkingGarage);

            static async Task ListenForTicketMachines(ParkingGarage parkingGarage)
            {
                Console.WriteLine("Waiting for ticket machine connection...");

                var clientTasks = new List<Task>();
                
                while (true)
                {
                    var pipeServer = new NamedPipeServerStream("ParkingGaragePipe",
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);
                    
                    await pipeServer.WaitForConnectionAsync();
                    Console.WriteLine("New ticket machine connected!");
                    
                    var clientTask = HandleClientAsync(pipeServer, parkingGarage);

                    clientTasks.RemoveAll(task => task.IsCompleted);
                    clientTasks.Add(clientTask);
                }
            }
            
            static async Task HandleClientAsync(NamedPipeServerStream pipeServer, ParkingGarage parkingGarage)
            {
                var clientId = Guid.NewGuid().ToString().Substring(0, 8);
                Console.WriteLine($"Client {clientId} connected.");

    
                try
                {
                    await using (pipeServer)
                    using (var reader = new StreamReader(pipeServer))
                    await using (var writer = new StreamWriter(pipeServer) { AutoFlush = true })
                    {
                        while (pipeServer.IsConnected)
                        {
                            var command = await reader.ReadLineAsync();
                            if (command == null) break;
                
                            Console.WriteLine($"Client {clientId}: Processing command: {command}");
                            var response = ProcessCommand(command, parkingGarage);
                            await writer.WriteLineAsync(response);
                            Console.WriteLine($"Client {clientId}: Sent response: {response}");
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Client {clientId} disconnected: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client {clientId}: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine($"Client {clientId} connection closed");
                }
            }

            static string ProcessCommand(string command, ParkingGarage parkingGarage)
            {
                var parts = command.Split('|');

                switch (parts[0])
                {
                    case "DISPENSE_TICKET":
                        return DispenseTicket(parkingGarage, parts);
                    case "VALIDATE_TICKET":
                        return ValidateTicket(parkingGarage, parts);
                    case "GET_AVAILABLE_SPOTS":
                        return GetNumberOfRemainingSpots(parkingGarage, parts);
                    case "FIND_TICKET_BY_LICENSE" :
                        return LookupTicketByLicencePlate(parkingGarage, parts);
                    default:
                        return "ERROR|Unknown command";
                }
            }
        }



        static string DispenseTicket(ParkingGarage parkingGarage, string[] requestParts)
        {
            if (requestParts.Count() >= 2)
            {
                VehicleType vehicleType = requestParts[1] == "Car" ? VehicleType.Car : VehicleType.Motorcycle;
                try
                {
                    var vehicle = new Vehicle(vehicleType);
                    var ticket = parkingGarage.DispenseTicket(vehicle);
                    return $"TICKET_ISSUED|{ticket.Id}|{ticket.SpotAssignment}|{ticket.TimeStamp}|{vehicle.Id}|{vehicle.Type}|{vehicle.LicensePlate}";
                }
                catch (InvalidOperationException ex)
                {
                    return $"ERROR|{ex.Message}";
                }
            }

            return "ERROR|Invalid DISPENSE_TICKET command format.";
        }

        static string ValidateTicket(ParkingGarage parkingGarage, string[] requestParts)
        {
            if (requestParts.Count() >= 2 && int.TryParse(requestParts[1], out var ticketId))
            {
                var response = parkingGarage.ValidateTicket(ticketId);
                return $"TICKET_VALIDATED|{response}";
            }

            return "ERROR|Invalid VALIDATE_TICKET command format.";
        }

        static string GetNumberOfRemainingSpots(ParkingGarage parkingGarage, string[] responseParts)
        {

            var response = parkingGarage.GetNumberOfRemainingSpots();
            return $"AVAILABLE_SPOTS|{response.TotalEmptySpots}|{response.MotorcycleOnlySpots}";
        }


        static string LookupTicketByLicencePlate(ParkingGarage parkingGarage, string[] responseParts)
        {
            if (responseParts.Count() >= 2)
            {
                var licencePlate = responseParts[1];
                var ticket = parkingGarage.FindTicketIdByLicencePlate(licencePlate);
                if (ticket != null)
                {
                    return $"TICKET_FOUND|{ticket.Id}|{ticket.SpotAssignment}|{ticket.TimeStamp}|{ticket.Vehicle.Id}|{ticket.Vehicle.Type}|{ticket.Vehicle.LicensePlate}";
                }
                else
                {
                    return "TICKET_NOT_FOUND";
                }
            }
            return "ERROR|Invalid VALIDATE_TICKET command format.";

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








