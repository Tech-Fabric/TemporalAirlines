using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Services.Models.Flight;

public class FlightDetailsModel
{
    public string Id { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public DateTime Depart { get; set; }

    public DateTime Arrival { get; set; }

    public Dictionary<string, Ticket> Seats { get; set; } = new();

    public List<Ticket> Registered { get; set; } = [];

    public List<Ticket> Boarded { get; set; } = [];

    public FlightStatus Status { get; set; }
}