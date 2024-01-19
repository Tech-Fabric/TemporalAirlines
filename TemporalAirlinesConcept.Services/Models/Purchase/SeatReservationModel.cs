using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class SeatReservationModel
{
    public string FlightId { get; set; }
    
    public Ticket Ticket { get; set; }
    
    public string Seat { get; set; }
}