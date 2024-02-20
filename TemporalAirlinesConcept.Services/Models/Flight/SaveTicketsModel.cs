using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Flight;

public class SaveTicketsModel
{
    public List<Ticket> Tickets { get; set; } = [];
}
