using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Client;
using Temporalio.Common;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

public class TicketService : ITicketService
{
    private readonly ITemporalClient _temporalClient;
    private readonly IFlightRepository _flightRepository;

    public TicketService(ITemporalClient temporalClient, IFlightRepository flightRepository)
    {
        _temporalClient = temporalClient;
        _flightRepository = flightRepository;
    }

    public async Task<string> RequestTicketPurchaseAsync(PurchaseModel purchaseModel)
    {
        foreach (var flightId in purchaseModel.FlightsId)
        {
            await CreateFlightWorkflowIfNotExistsAsync(flightId);
        }

        var workflowId = Guid.NewGuid().ToString();
        
        await _temporalClient.StartWorkflowAsync<PurchaseWorkflow>(
            wf => wf.RunAsync(purchaseModel), new WorkflowOptions
            {
                TaskQueue = Temporal.DefaultQueue,
                Id = workflowId,
                RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 1,
                    InitialInterval = TimeSpan.FromMinutes(5),
                    BackoffCoefficient = 2
                }
            });

        return workflowId;
    }

    private async Task CreateFlightWorkflowIfNotExistsAsync(string flightId)
    {
        if (await WorkflowHandleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, flightId))
            return;
            
        var flight = await _flightRepository.GetFlightAsync(flightId);

        if (flight is null)
            throw new EntityNotFoundException($"Flight {flightId} was not found.");

        await _temporalClient.StartWorkflowAsync((FlightWorkflow wf) => wf.RunAsync(flight),
            new WorkflowOptions(flightId, Temporal.DefaultQueue));
    }
}