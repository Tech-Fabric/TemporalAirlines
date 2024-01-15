using TemporalAirlinesConcept.Services.Models.User;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Interfaces.UserRegistration;

[Workflow]
public interface IUserRegistrationWorkflow
{
    [WorkflowRun]
    Task<bool> Run(UserRegistrationModel registrationModel);
}
