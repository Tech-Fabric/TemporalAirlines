namespace TemporalAirlinesConcept.Api.Models.Tickets;

public class StartPurchaseRequest
{
    public string UserId { get; set; }

    public string FlightId { get; set; }

    public int NumberOfTickets { get; set; }
}
