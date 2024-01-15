using TemporalAirlinesConcept.Services.Interfaces.UserRegistration;
using TemporalAirlinesConcept.Services.Models.User;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.UserRegistration;

[Workflow]
public class UserRegistrationWorkflow : IUserRegistrationWorkflow
{
    private readonly ActivityOptions _options = new()
    {
        ScheduleToCloseTimeout = TimeSpan.FromSeconds(45),
        RetryPolicy = new Temporalio.Common.RetryPolicy
        {
            MaximumAttempts = 3
        }
    };

    [WorkflowRun]
    public async Task<bool> Run(UserRegistrationModel registrationModel)
    {
        var isInputValid = ValidateUser(registrationModel);

        if (!isInputValid)
            return false;

        await Workflow.ExecuteActivityAsync(
            (UserRegistrationActivities act) => act.SendErrorConfirmationCode(),
            _options);

        await Workflow.DelayAsync(TimeSpan.FromMinutes(2));

        await Workflow.ExecuteActivityAsync(
            (UserRegistrationActivities act) => act.CreateUser(registrationModel),
            _options);

        return true;
    }

    private bool ValidateUser(UserRegistrationModel registrationModel)
    {
        var isInputModelValid = !string.IsNullOrEmpty(registrationModel?.Name)
            && !string.IsNullOrEmpty(registrationModel?.Email);

        return isInputModelValid;
    }
}
