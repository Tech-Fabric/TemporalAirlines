using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Models.Seat;

namespace TemporalAirlinesConcept.Api.Models.Flights;

public class FlightResponse
{
    public string Id { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public DateTime Depart { get; set; }

    public DateTime Arrival { get; set; }

    public decimal Price { get; set; }

    public List<Seat> Seats { get; set; }

    public List<string> Registered { get; set; }

    public List<string> Boarded { get; set; }

    public FlightStatus Status { get; set; }
}
