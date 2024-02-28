using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Services.Models.Flight;

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