using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class MarkTicketPaidRequestModel
{
    public Ticket Ticket { get; set; }
}