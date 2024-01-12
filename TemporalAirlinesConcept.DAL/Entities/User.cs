using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TemporalAirlinesConcept.DAL.Entities;

public class User
{
    public const string Container = "users";

    [Key]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; }

    public string Email { get; set; }

    public string Role { get; set; }
}