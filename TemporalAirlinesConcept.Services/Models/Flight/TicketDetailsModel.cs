using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Services.Models.Flight;

public class TicketDetailsModel
{
    public Guid Id { get; set; }

    public Guid? FlightId { get; set; }

    public Guid? UserId { get; set; }

    public string PurchaseId { get; set; }

    public string Passenger { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public BoardingStatus BoardingStatus { get; set; }

    public string Seat { get; set; }

    #region Methods

    public override bool Equals(object obj)
    {
        if (obj is not TicketDetailsModel ticket)
            return false;

        var comparisonResult = Id == ticket.Id
                               && FlightId == ticket.FlightId
                               && PurchaseId == ticket.PurchaseId
                               && UserId == ticket.UserId
                               && string.Equals(Passenger, ticket.Passenger, StringComparison.OrdinalIgnoreCase)
                               && PaymentStatus == ticket.PaymentStatus
                               && BoardingStatus == ticket.BoardingStatus
                               && string.Equals(Seat, ticket.Seat, StringComparison.OrdinalIgnoreCase);

        return comparisonResult;
    }

    #endregion
}
