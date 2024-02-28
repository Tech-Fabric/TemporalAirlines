using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Services.Models.Flight;

public class TicketDetailsModel
{
    public Guid Id { get; set; }

    public Guid? FlightId { get; set; }

    public Guid? UserId { get; set; }

    public string PurchaseId { get; set; }

    public string Passenger { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public BoardingStatus BoardingStatus { get; set; }

    public string Seat { get; set; }
}
