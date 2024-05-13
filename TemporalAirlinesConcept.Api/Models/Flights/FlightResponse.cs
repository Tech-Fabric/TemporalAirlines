using TemporalAirlinesConcept.Api.Models.Tickets;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Api.Models.Flights;

public class FlightResponse
{
    public string Id { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public DateTime Depart { get; set; }

    public DateTime Arrival { get; set; }

    public decimal Price { get; set; }

    public List<SeatResponse> Seats { get; set; }

    public List<TicketResponse> Tickets { get; set; }

    public FlightStatus Status { get; set; }
}
