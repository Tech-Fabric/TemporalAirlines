using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Entities;

public class User : IEntity<Guid>
{
    #region Properties

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }

    public string Email { get; set; }

    public string Role { get; set; }

    #endregion

    #region Navigation Properties

    [InverseProperty("User")]
    public virtual ICollection<Ticket> Tickets { get; set; } = [];

    #endregion
}