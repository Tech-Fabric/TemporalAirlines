using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class BookingModel
{
    public string FlightId { get; set; }
    
    public Ticket Ticket { get; set; }
}