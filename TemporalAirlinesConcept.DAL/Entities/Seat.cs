using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TemporalAirlinesConcept.DAL.Entities;

public class Seat
{
    #region Properties

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }

    public decimal Price { get; set; }

    public Guid? TicketId { get; set; }

    public Guid FlightId { get; set; }

    #endregion

    #region Navigation Properties

    [ForeignKey("TicketId")]
    public virtual Ticket Ticket { get; set; }

    [ForeignKey("FlightId")]
    [InverseProperty("Seats")]
    public virtual Flight Flight { get; set; }

    #endregion

    #region Methods

    public override bool Equals(object obj)
    {
        if (obj is not Seat seat)
            return false;

        var comparisonResult = Id == seat.Id
            && string.Equals(Name, seat.Name, StringComparison.OrdinalIgnoreCase)
            && TicketId == seat.TicketId
            && Price == seat.Price;

        return comparisonResult;
    }

    #endregion
}
