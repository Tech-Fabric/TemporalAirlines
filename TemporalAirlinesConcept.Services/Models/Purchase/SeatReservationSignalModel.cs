using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class SeatReservationSignalModel
{
    public SeatReservationSignalModel(Ticket ticket, string seat)
    {
        Ticket = ticket;
        Seat = seat;
    }
    
    public Ticket Ticket { get; set; }
    
    public string Seat { get; set; }
}