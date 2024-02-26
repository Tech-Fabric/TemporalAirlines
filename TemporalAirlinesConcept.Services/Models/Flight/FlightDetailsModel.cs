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

public class SeatDetailsModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public Guid? TicketId { get; set; }

    public Guid FlightId { get; set; }
}

public class FlightDetailsModel
{
    public Guid Id { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public DateTime? Depart { get; set; }

    public DateTime? Arrival { get; set; }

    public decimal? Price { get; set; }

    public List<SeatDetailsModel> Seats { get; set; } = [];

    public List<TicketDetailsModel> Registered { get; set; } = [];
    
    public List<TicketDetailsModel> Boarded { get; set; } = [];

    public FlightStatus Status { get; set; }
}