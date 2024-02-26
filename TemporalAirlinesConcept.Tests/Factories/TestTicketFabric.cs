using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Tests.Factories;

public class TestTicketFabric
{
    public static TicketDetailsModel GetTestTicket(Guid flightId,
        string passenger = "passenger", 
        PaymentStatus status = PaymentStatus.Pending)
    {
        return new TicketDetailsModel
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PaymentStatus = status,
            FlightId = flightId,
            Passenger = passenger,
        };
    }
}