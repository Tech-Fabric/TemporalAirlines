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
    public async Task<bool> RunAsync(PurchaseModel purchaseModel)
    {
        var activityOptions = new ActivityOptions
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

        try
        {
            var isFlightAvailable =
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.IsFlightAvailableAsync(purchaseModel.FlightsId), activityOptions);

            if (!isFlightAvailable) return false;

            var tickets = new List<Ticket>();
            
            foreach (var flightId in purchaseModel.FlightsId)
            {
                var handle = Workflow.GetExternalWorkflowHandle<FlightWorkflow>(flightId);

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid().ToString(),
                    FlightId = flightId,
                    Passenger = purchaseModel.Passenger,
                    UserId = purchaseModel.UserId,
                    Seat = null,
                    PaymentStatus = PaymentStatus.Pending
                };
                
                tickets.Add(ticket);

                await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.CreateTicketAsync(ticket),
                    activityOptions);

                saga.AddCompensation(async () =>
                    await Workflow.ExecuteActivityAsync(
                        (PurchaseActivities act) => act.CreateTicketCompensationAsync(ticket), activityOptions));

                await handle.SignalAsync(wf => wf.BookSeatAsync(ticket.Id));

                saga.AddCompensation(
                    async () => await handle.SignalAsync(wf => wf.BookSeatCompensationAsync(ticket.Id)));
            }
            
            var isPaid = await Workflow.WaitConditionAsync(() => _isPaid, TimeSpan.FromMinutes(15));

            if (!isPaid) throw new ApplicationException("Tickets was not paid in 15 min.");

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyAsync(), activityOptions);

            saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyCompensationAsync(), activityOptions));
            
            foreach (var ticket in tickets)
            {
                await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.MarkTicketPaidAsync(ticket.Id),
                    activityOptions);
            }

            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GenerateTicketsAsync(), activityOptions);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.GenerateTicketsCompensationAsync(), activityOptions));

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SendTicketsAsync(), activityOptions);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.SendTicketsCompensationAsync(), activityOptions));
            
            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.ConfirmWithdrawAsync(),
                activityOptions);
            
            var lastFlight = await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GetLastFlightAsync(purchaseModel.FlightsId.ToArray()), activityOptions);

            var sleepDuration = lastFlight.Depart.Subtract(Workflow.UtcNow);

            var isCancelled = await Workflow.WaitConditionAsync(() => _isCancelled, sleepDuration);

            if (isCancelled)
            {
                throw new Exception("Purchase has being cancelled");
            }

            return true;
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
