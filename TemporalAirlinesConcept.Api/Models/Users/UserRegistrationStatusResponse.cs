namespace TemporalAirlinesConcept.Api.Models.Users;

public class UserRegistrationStatusResponse
{
    public Dictionary<string, List<string>> ValidationErrors { get; set; }

    public UserResponse CreatedUser { get; set; }

    public bool IsAnyErrors { get; set; }

    public bool IsUserCreated { get; set; }
}
