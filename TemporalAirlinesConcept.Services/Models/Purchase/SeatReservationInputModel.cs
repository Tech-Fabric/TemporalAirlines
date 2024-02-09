using TemporalAirlinesConcept.DAL.Models.Seat;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class SeatReservationInputModel
{
    public string FlightId { get; set; }
    
    public string TicketId { get; set; }
    
    public Seat Seat { get; set; }
}