using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Entities;

public class Ticket : IEntity<Guid>
{
    #region Properties

    [Key]
    public Guid Id { get; set; }

    public Guid? FlightId { get; set; }

    public Guid? UserId { get; set; }

    public string PurchaseId { get; set; }

    public string Passenger { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public BoardingStatus BoardingStatus { get; set; }

    #endregion

    #region Navigation Properties
    
    [InverseProperty("Ticket")]
    public Seat Seat { get; set; }

    [ForeignKey("FlightId")]
    [InverseProperty("Tickets")]
    public Flight Flight { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Tickets")]
    public User User { get; set; }

    #endregion

    #region Methods

    public override bool Equals(object obj)
    {
        if (obj is not Ticket ticket)
            return false;

        var comparisonResult = Id == ticket.Id
            && FlightId == ticket.FlightId
            && UserId == ticket.UserId
            && string.Equals(Passenger, ticket.Passenger, StringComparison.OrdinalIgnoreCase)
            && PaymentStatus == ticket.PaymentStatus;

        return comparisonResult;
    }

    #endregion
}