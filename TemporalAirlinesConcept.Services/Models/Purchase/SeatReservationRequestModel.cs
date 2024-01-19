using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class SeatReservationRequestModel
{
    public Ticket Ticket { get; set; }
    
    public string Seat { get; set; }
}