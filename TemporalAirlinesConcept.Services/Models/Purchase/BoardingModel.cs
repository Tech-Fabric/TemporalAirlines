using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class BoardingModel
{
    public string FlightId { get; set; }
    
    public Ticket Ticket { get; set; }
}