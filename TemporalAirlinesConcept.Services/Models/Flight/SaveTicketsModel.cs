using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Flight;

public class SaveTicketsModel
{
    public List<TicketDetailsModel> Tickets { get; set; } = [];
}
