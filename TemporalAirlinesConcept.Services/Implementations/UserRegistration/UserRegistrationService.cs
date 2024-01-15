using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Services.Interfaces.UserRegistration;
using TemporalAirlinesConcept.Services.Models.User;
using Temporalio.Client;
using Temporalio.Common;

namespace TemporalAirlinesConcept.Services.Implementations.UserRegistration
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly ITemporalClient _temporalClient;

        public UserRegistrationService(ITemporalClient temporalClient)
        {
            _temporalClient = temporalClient;
        }

        public async Task RegisterUser(UserRegistrationModel registrationModel)
        {
            var handle = await _temporalClient.StartWorkflowAsync<IUserRegistrationWorkflow>(
            wf => wf.Run(registrationModel),
            new WorkflowOptions
            {
                //TaskQueue = Temporal.UserRegistrationQueue,
                TaskQueue = Temporal.DefaultQueue,
                Id = Guid.NewGuid().ToString(),
                RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3
                }
            });
        }
    }
}
