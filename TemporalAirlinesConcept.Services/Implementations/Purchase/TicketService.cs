using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Client;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

public class TicketService : ITicketService
{
    private readonly ITemporalClient _temporalClient;

    public TicketService(ITemporalClient temporalClient)
    {
        _temporalClient = temporalClient;
    }

    public async Task<List<DAL.Entities.Ticket>> PurchaseTicketAsync(PurchaseInputModel purchaseRequestModel)
    {
        //todo: pull user id from authorization

        var handle = await _temporalClient.StartWorkflowAsync<PurchaseWorkflow>(
            wf => wf.RunAsync(new PurchaseModel
            {
                FlightsId = purchaseRequestModel.FlightsId,
                Passenger = purchaseRequestModel.PassengerDetails.Name,
                UserId = "user id"
            }), new WorkflowOptions { TaskQueue = "flight-task-queue", Id = Guid.NewGuid().ToString() });

        return await handle.GetResultAsync<List<DAL.Entities.Ticket>>();
    }
}