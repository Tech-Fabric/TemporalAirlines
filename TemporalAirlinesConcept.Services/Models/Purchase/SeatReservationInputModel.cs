namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class SeatReservationInputModel
{
    public string FlightId { get; set; }
    
    public string PurchaseId { get; set; }
    
    public List<string> Seats { get; set; }
}