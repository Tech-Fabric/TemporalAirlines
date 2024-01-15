using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class PurchaseInputModel
{
    public List<string> FlightsId { get; set; }

    public PassengerDetails PassengerDetails { get; set; }
}