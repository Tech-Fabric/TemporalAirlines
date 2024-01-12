namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class PurchaseModel
{
    public string UserId { get; set; }

    public List<string> FlightsId { get; set; }

    public string Passenger { get; set; } //name todo: change to PassengerDetails
}