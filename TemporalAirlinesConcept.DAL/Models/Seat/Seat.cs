namespace TemporalAirlinesConcept.DAL.Models.Seat;

public class Seat
{
    public string Name { get; set; }
    
    public decimal Price { get; set; }
    
    public string TicketId { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not Seat seat)
            return false;

        return Name.Equals(seat.Name) &&
               Price.Equals(seat.Price) &&
               TicketId.Equals(seat.TicketId);
    }
}