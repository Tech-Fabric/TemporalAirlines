using System.Net.NetworkInformation;
using TemporalAirlinesConcept.Services.Models.UserRegistration;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Interfaces.UserRegistration;

[Workflow]
public interface IUserRegistrationWorkflow
{
    [WorkflowRun]
    Task<UserRegistrationStatus> Run(UserRegistrationModel registrationModel);

    [WorkflowQuery]
    UserRegistrationStatus GetStatus();
}
