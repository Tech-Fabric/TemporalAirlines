namespace TemporalAirlinesConcept.Services.Models.UserRegistration
{
    public class UserRegistrationStatus
    {
        public Dictionary<string, List<string>> ValidationErrors { get; set; }

        public DAL.Entities.User CreatedUser { get; set; }

        public bool IsAnyErrors
        {
            get => ValidationErrors.Any(x => x.Value.Count > 0);
        }
    }
}
