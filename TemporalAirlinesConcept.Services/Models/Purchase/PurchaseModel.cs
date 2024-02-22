namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class PurchaseModel
{
    public Guid UserId { get; set; }

    public Guid FlightId { get; set; }

    public int NumberOfTickets { get; set; }
}
