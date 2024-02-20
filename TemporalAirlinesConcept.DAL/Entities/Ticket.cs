using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Models.Seat;

namespace TemporalAirlinesConcept.DAL.Entities;

public class Ticket
{
    public const string Container = "tickets";

    [Key]
    [JsonProperty("id")]
    public string Id { get; set; }

    public string FlightId { get; set; }

    public string UserId { get; set; }

    public string PurchaseId { get; set; }
    
    public string Passenger { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public Seat Seat { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not Ticket ticket)
            return false;

        var comparisonResult = string.Equals(Id, ticket.Id, StringComparison.OrdinalIgnoreCase)
            && string.Equals(FlightId, ticket.FlightId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(UserId, ticket.UserId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Passenger, ticket.Passenger, StringComparison.OrdinalIgnoreCase)
            && PaymentStatus == ticket.PaymentStatus;

        return comparisonResult;
    }
}