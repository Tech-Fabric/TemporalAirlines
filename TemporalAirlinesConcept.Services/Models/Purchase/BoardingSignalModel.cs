using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class BoardingSignalModel
{
    public BoardingSignalModel()
    {
        
    }
    
    public BoardingSignalModel(Ticket ticket)
    {
        Ticket = ticket;
    }
    
    public Ticket Ticket { get; set; }
}