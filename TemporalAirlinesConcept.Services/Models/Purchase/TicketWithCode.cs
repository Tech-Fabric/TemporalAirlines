using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class TicketWithCode
{
    public Guid Id { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public Seat Seat { get; set; }

    public string Code { get; set; }
}
