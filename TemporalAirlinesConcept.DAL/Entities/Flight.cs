using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Entities;

public class Flight : IEntity<Guid>
{
    #region Properties

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string From { get; set; }

    public string To { get; set; }

    public DateTime Depart { get; set; }

    public DateTime Arrival { get; set; }

    public decimal Price { get; set; }

    public FlightStatus Status { get; set; } = FlightStatus.Pending;

    #endregion

    #region Navigation Properties

    [InverseProperty("Flight")]
    public virtual ICollection<Seat> Seats { get; set; }

    [InverseProperty("Flight")]
    public virtual ICollection<Ticket> Tickets { get; set; }

    #endregion
}
