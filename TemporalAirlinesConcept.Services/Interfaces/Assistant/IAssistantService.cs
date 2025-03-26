using TemporalAirlinesConcept.Services.Models.Assistant;

namespace TemporalAirlinesConcept.Services.Interfaces.Assistant;

public interface IAssistantService
{
    Task<AssistantAnswer> ProcessUserRequest(RequestToAssistant request);
}
