using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.DAL.Entities;

public class Flight
{
    public const string Container = "flights";

    [Key]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string From { get; set; }

    public string To { get; set; }

    public DateTime Depart { get; set; }

    public DateTime Arrival { get; set; }

    public Dictionary<string, string> Seats { get; set; }

    public List<string> Registered { get; set; } = [];

    public List<string> Boarded { get; set; } = [];

    public FlightStatus Status { get; set; } = FlightStatus.Pending;
}
