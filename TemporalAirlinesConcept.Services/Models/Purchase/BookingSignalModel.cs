using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class BookingSignalModel
{
    public BookingSignalModel()
    {
        
    }
    
    public BookingSignalModel(Ticket ticket)
    {
        Ticket = ticket;
    }
    
    public Ticket Ticket { get; set; }
}