using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class PurchaseInputModel
{
    public string UserId { get; set; }
    
    public List<string> FlightsId { get; set; }

    public PassengerDetails PassengerDetails { get; set; }
}