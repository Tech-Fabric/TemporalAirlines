namespace TemporalAirlinesConcept.Api.Models.Flights;

public class SeatResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public Guid? TicketId { get; set; }

    public Guid FlightId { get; set; }
}
