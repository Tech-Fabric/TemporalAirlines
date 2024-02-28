namespace TemporalAirlinesConcept.Services.Models.Flight;

public class SeatDetailsModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public Guid? TicketId { get; set; }

    public Guid FlightId { get; set; }
}