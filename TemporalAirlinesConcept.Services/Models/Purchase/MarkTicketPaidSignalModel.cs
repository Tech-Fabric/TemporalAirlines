using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class MarkTicketPaidSignalModel
{
    public MarkTicketPaidSignalModel()
    {
        
    }

    public MarkTicketPaidSignalModel(Ticket ticket)
    {
        Ticket = ticket;
    }
    
    public Ticket Ticket { get; set; }
}