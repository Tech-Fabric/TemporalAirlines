using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Tests.Factories;

public class TestTicketFabric
{
    public static Ticket GetTestTicket(string flightId,
        string passenger = "passenger", PaymentStatus status = PaymentStatus.Pending)
    {
        return new Ticket
        { 
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            PaymentStatus = status,
            FlightId = flightId,
            Passenger = passenger,
        };
    }
}