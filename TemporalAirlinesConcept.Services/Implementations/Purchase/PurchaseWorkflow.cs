using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Common;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

[Workflow]
public class PurchaseWorkflow
{
    private bool _isPaid = false;

    private bool _isCancelled = false;

    [WorkflowRun]
    public async Task<List<Ticket>> RunAsync(PurchaseModel purchaseModel)
    {
        var options = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromSeconds(60),
            RetryPolicy = new RetryPolicy
            {
                InitialInterval = TimeSpan.FromSeconds(15),
                BackoffCoefficient = 1,
                MaximumAttempts = 2
            }
        };

        var saga = new Saga([]);

        var userId = purchaseModel.UserId;
        var flightsId = purchaseModel.FlightsId;
        var passenger = purchaseModel.Passenger;

        try
        {
            foreach (var flightId in flightsId)
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FlightId = flightId,
                    Passenger = passenger,
                    PaymentStatus = PaymentStatus.Pending
                };

                var handle = Workflow.GetExternalWorkflowHandle<FlightWorkflow>(flightId);

                await handle.SignalAsync(wf => wf.BookSeatsAsync(new[] { ticket }));
                
                saga.AddCompensation(async () => await handle.SignalAsync(wf => 
                    wf.RemoveSeatsBookingAsync(new []{ticket})));
            }

            var isPaid = await Workflow.WaitConditionAsync(() => _isPaid, TimeSpan.FromMinutes(15));

            if (!isPaid) throw new ApplicationException("Tickets was not paid in 15 min.");

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyAsync(userId, flightsId), options);

            saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyCompensationAsync(userId, flightsId), options));

            foreach (var handle in flightsId.Select(flightId => Workflow.GetExternalWorkflowHandle<FlightWorkflow>(flightId)))
            {
                await handle.SignalAsync(wf => wf.MarkTicketAsPaidAsync(userId, passenger));
            }

            var blobTickets = await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GenerateTicketsAsync(userId, flightsId, passenger), options);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.GenerateTicketsCompensationAsync(blobTickets), options));

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SendTicketsAsync(userId, blobTickets), options);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.SendTicketsCompensationAsync(userId, blobTickets), options));

            var tickets = await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SaveTicketsAsync(blobTickets), options);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.SaveTicketsCompensationAsync(tickets), options));
            
            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.ConfirmWithdrawAsync(userId, flightsId),
                options);
            
            var lastFlight = await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GetLastFlightAsync(flightsId.ToArray()), options);

            var sleepDuration = lastFlight.Depart.Subtract(Workflow.UtcNow);

            var isCancelled = await Workflow.WaitConditionAsync(() => _isCancelled, sleepDuration);

            if (isCancelled)
            {
                //start cancellation workflow
            }

            return tickets;
        }
        catch (Exception)
        {
            saga.OnCompensationError((log) =>
            {
                log.Add("Compensation error. Manual intervention required!");
                
                return Task.CompletedTask;
            });

            saga.OnCompensationComplete((log) =>
            {
                log.Add("Compensation completed successfully");
                
                return Task.CompletedTask;
            });

            await saga.CompensateAsync();

            throw;
        }
    }

    [WorkflowSignal]
    public Task SetPaidStatus()
    {
        if (_isPaid is not true) _isPaid = true;

        return Task.CompletedTask;
    }

    [WorkflowSignal]
    public Task Cancel()
    {
        if (_isCancelled is not true) _isCancelled = true;

        return Task.CompletedTask;
    }
}
