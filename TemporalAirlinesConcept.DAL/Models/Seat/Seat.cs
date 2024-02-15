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

        var comparisonResult = string.Equals(Name, seat.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(TicketId, seat.TicketId, StringComparison.OrdinalIgnoreCase)
            && Price == seat.Price;

        return comparisonResult;
    }
}