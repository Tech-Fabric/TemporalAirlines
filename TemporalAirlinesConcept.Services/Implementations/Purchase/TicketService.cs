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

    public async Task<List<DAL.Entities.Ticket>> PurchaseTicketAsync(PurchaseInputModel purchaseRequestModel)
    {
        foreach (var flightId in purchaseRequestModel.FlightsId)
        {
            if (!await WorkflowHandleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, flightId))
            {
                var flight = await _flightRepository.GetFlightAsync(flightId);

                if (flight is null)
                    throw new EntityNotFoundException($"Flight {flightId} was not found.");

                await _temporalClient.StartWorkflowAsync((FlightWorkflow wf) => wf.RunAsync(flight),
                    new WorkflowOptions(flightId, "my-task-queue"));
            }

            var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

            var availableSeats = await flightHandle.QueryAsync(wf => wf.AvailableSeats());

            if (availableSeats < 1)
                throw new InvalidOperationException($"No free seats for flight {flightId}.");
        }
        
        var handle = await _temporalClient.StartWorkflowAsync<PurchaseWorkflow>(
            wf => wf.RunAsync(new PurchaseModel
            {
                FlightsId = purchaseRequestModel.FlightsId,
                Passenger = purchaseRequestModel.PassengerDetails.Name,
                UserId = "user id"
            }), new WorkflowOptions { TaskQueue = "my-task-queue", Id = Guid.NewGuid().ToString() });

        return await handle.GetResultAsync<List<DAL.Entities.Ticket>>();
    }
}