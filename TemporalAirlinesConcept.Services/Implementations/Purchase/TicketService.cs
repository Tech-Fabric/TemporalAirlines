using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Client;

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

    public async Task RequestTicketPurchaseAsync(PurchaseInputModel purchaseRequestModel)
    {
        //create flight workflow if not exists
        foreach (var flightId in purchaseRequestModel.FlightsId)
        {
            if (await WorkflowHandleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, flightId)) continue;
            
            var flight = await _flightRepository.GetFlightAsync(flightId);

            if (flight is null)
                throw new EntityNotFoundException($"Flight {flightId} was not found.");

            await _temporalClient.StartWorkflowAsync((FlightWorkflow wf) => wf.RunAsync(flight),
                new WorkflowOptions(flightId, Temporal.DefaultQueue));
        }
        
        //start purchase workflow
        await _temporalClient.StartWorkflowAsync<PurchaseWorkflow>(
            wf => wf.RunAsync(new PurchaseModel
            {
                FlightsId = purchaseRequestModel.FlightsId,
                Passenger = purchaseRequestModel.PassengerDetails.Name,
                UserId = purchaseRequestModel.UserId
            }), new WorkflowOptions { TaskQueue = Temporal.DefaultQueue, Id = Guid.NewGuid().ToString() });
    }
}