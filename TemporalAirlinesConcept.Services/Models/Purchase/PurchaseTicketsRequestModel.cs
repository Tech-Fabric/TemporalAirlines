namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class PurchaseTicketsRequestModel
{
    public Guid FlightId { get; set; }
    
    public string PurchaseId { get; set; }
}