using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Services.Interfaces.UserRegistration;
using TemporalAirlinesConcept.Services.Models.UserRegistration;
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

        public async Task ConfirmUser(string registrationId)
        {
            var registrationHandle = GetWorkflow<IUserRegistrationWorkflow>(registrationId);

            await registrationHandle.SignalAsync(x => x.Confirm());
        }

        public async Task<UserRegistrationStatus> GetUserRegistrationInfo(string registrationId)
        {
            var registrationHandle = GetWorkflow<IUserRegistrationWorkflow>(registrationId);

            var status = await registrationHandle.QueryAsync(x => x.GetStatus());

            return status;
        }

        public async Task<string> RegisterUser(UserRegistrationModel registrationModel)
        {
            var handle = await _temporalClient.StartWorkflowAsync<IUserRegistrationWorkflow>(
                wf => wf.Run(registrationModel),
                new WorkflowOptions
                {
                    TaskQueue = Temporal.DefaultQueue,
                    Id = Guid.NewGuid().ToString(),
                    RetryPolicy = new RetryPolicy
                    {
                    }
                });

            return handle.Id;
        }

        private WorkflowHandle<T> GetWorkflow<T>(string id)
        {
            var handle = _temporalClient.GetWorkflowHandle<T>(id);

            return handle;
        }
    }
}
