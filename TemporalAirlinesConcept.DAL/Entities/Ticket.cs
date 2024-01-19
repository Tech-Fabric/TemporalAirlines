using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.DAL.Entities;

public class Ticket
{
    public const string Container = "tickets";

    [Key]
    [JsonProperty("id")]
    public string Id { get; set; }

    public string FlightId { get; set; }

    public string UserId { get; set; }

    public string Passenger { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string Seat { get; set; }
}