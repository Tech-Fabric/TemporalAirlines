using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Tests.Factories;

public class TestFlightFabric
{
    public static Flight GetTestFlight(DateTime? depart = null, DateTime? arrival = null, 
        string from = "from", string to = "to", List<Seat>? seats = null)
    {
        depart ??= DateTime.UtcNow.AddHours(25);
        arrival ??= DateTime.UtcNow.AddDays(2);
        seats ??= [
            new Seat {Name = "1", Price = 100}, 
            new Seat {Name = "2", Price = 200}, 
            new Seat {Name = "3", Price = 150}
        ];
        
        return new Flight
        {
            Id = Guid.NewGuid(),
            Status = FlightStatus.Pending,
            Depart = (DateTime)depart,
            Arrival = (DateTime)arrival,
            From = from,
            To = to,
            Seats = seats,
        };
    }
}