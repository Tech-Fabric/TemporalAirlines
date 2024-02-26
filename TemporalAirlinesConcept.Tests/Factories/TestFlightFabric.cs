using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Tests.Factories;

public class TestFlightFabric
{
    public static FlightDetailsModel GetTestFlight(DateTime? depart = null,
        DateTime? arrival = null,
        string from = "from",
        string to = "to",
        List<SeatDetailsModel> seats = null)
    {
        depart ??= DateTime.UtcNow.AddHours(25);
        arrival ??= DateTime.UtcNow.AddDays(2);
        seats ??= [
            new SeatDetailsModel { Name = "1", Price = 100 },
            new SeatDetailsModel { Name = "2", Price = 200 },
            new SeatDetailsModel { Name = "3", Price = 150 }
        ];

        return new FlightDetailsModel
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