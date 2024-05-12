namespace TemporalAirlinesConcept.Api.Models.Tickets;

public class StartPurchaseRequest
{
    public Guid UserId { get; set; }

    public Guid FlightId { get; set; }

    public int NumberOfTickets { get; set; }
}
