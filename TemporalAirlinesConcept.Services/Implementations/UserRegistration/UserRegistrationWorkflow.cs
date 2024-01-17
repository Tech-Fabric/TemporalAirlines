using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Services.Interfaces.UserRegistration;
using TemporalAirlinesConcept.Services.Models.UserRegistration;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.UserRegistration;

[Workflow]
public class UserRegistrationWorkflow : IUserRegistrationWorkflow
{
    private UserRegistrationStatus _status;

    private readonly ActivityOptions _options = new()
    {
        ScheduleToCloseTimeout = TimeSpan.FromSeconds(60),
        RetryPolicy = new Temporalio.Common.RetryPolicy
        {
        }
    };

    [WorkflowRun]
    public async Task<UserRegistrationStatus> Run(UserRegistrationModel registrationModel)
    {
        _status = new UserRegistrationStatus
        {
            ValidationErrors = ValidateUser(registrationModel)
        };

        if (_status.IsAnyErrors)
            return _status;

        await Workflow.ExecuteActivityAsync(
            (UserRegistrationActivities act) => act.SendConfirmationCode(),
            _options);

        await Workflow.DelayAsync(TimeSpan.FromMinutes(1));

        var createdUser = await Workflow.ExecuteActivityAsync(
            (UserRegistrationActivities act) => act.CreateUser(registrationModel),
            _options);

        _status.CreatedUser = createdUser;

        return _status;
    }

    [WorkflowQuery]
    public UserRegistrationStatus GetStatus() => _status;

    private Dictionary<string, List<string>> ValidateUser(UserRegistrationModel registrationModel)
    {
        var nameModelProperty = nameof(registrationModel.Name);
        var emailModelProperty = nameof(registrationModel.Email);

        Dictionary<string, List<string>> errors = new()
        {
            {
                nameModelProperty,
                []
            },
            {
                emailModelProperty,
                []
            }
        };

        if (string.IsNullOrEmpty(registrationModel.Name))
            errors[nameModelProperty].Add(UserRegistrationErrors.NameIsEmpty);

        if (string.IsNullOrEmpty(registrationModel.Email))
            errors[emailModelProperty].Add(UserRegistrationErrors.EmailIsEmpty);

        return errors;
    }
}
