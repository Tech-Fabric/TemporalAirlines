using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Tests.Factories;

public static class TestFlightFabric
{
    public static FlightDetailsModel GetTestFlight(DateTime? depart = null,
        DateTime? arrival = null,
        string from = "from",
        string to = "to",
        List<SeatDetailsModel> seats = null)
    {
        var flightId = Guid.NewGuid();
        
        depart ??= DateTime.UtcNow.AddHours(25);
        arrival ??= DateTime.UtcNow.AddDays(2);
        seats ??= [
            new SeatDetailsModel { Id = Guid.NewGuid(), Name = "1", Price = 100, FlightId = flightId },
            new SeatDetailsModel { Id = Guid.NewGuid(), Name = "2", Price = 200, FlightId = flightId },
            new SeatDetailsModel { Id = Guid.NewGuid(), Name = "3", Price = 150, FlightId = flightId }
        ];

        return new FlightDetailsModel
        {
            Id = flightId,
            Status = FlightStatus.Pending,
            Depart = (DateTime)depart,
            Arrival = (DateTime)arrival,
            From = from,
            To = to,
            Seats = seats
        };
    }
}