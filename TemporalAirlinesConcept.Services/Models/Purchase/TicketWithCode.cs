using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Models.Seat;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class TicketWithCode
{
    public string Id { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public Seat Seat { get; set; }

    public string Code { get; set; }
}
