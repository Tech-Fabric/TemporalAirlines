using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class BoardingSignalModel
{
    public BoardingSignalModel()
    {
        
    }
    
    public BoardingSignalModel(TicketDetailsModel ticket)
    {
        Ticket = ticket;
    }
    
    public TicketDetailsModel Ticket { get; set; }
}