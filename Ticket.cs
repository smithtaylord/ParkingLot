using ParkingLot.Utilities;

namespace ParkingLot;

public class Ticket
{
 public int Id { get; set; }
 public DateTime TimeStamp { get; set; }
 public string SpotAssignment { get; set; }
 public Vehicle Vehicle { get; set; }
 
 private const string NEXT_ID_FILE_PATH = "Output/Ticket/next_id.txt";
 private static int _nextId = NextIDService.LoadNextIdState(NEXT_ID_FILE_PATH);

 public Ticket(string spotAssignment, Vehicle vehicle)
 {
  Id = _nextId;
  TimeStamp = DateTime.Now;
  SpotAssignment = spotAssignment;
  Vehicle = vehicle;
  
  
  _nextId++;
  NextIDService.SaveNextIdState(_nextId, NEXT_ID_FILE_PATH);
 }
 
 public Ticket(int id, DateTime timestamp, string spotAssignment, Vehicle vehicle)
 {
  Id = id;
  TimeStamp = timestamp;
  SpotAssignment = spotAssignment;
  Vehicle = vehicle;
 }
 

}