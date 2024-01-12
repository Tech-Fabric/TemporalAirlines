using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Common;
using Temporalio.Exceptions;
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
        //retry if activity does not return within 60 seconds, first try will occur in 15 seconds,
        //  double delay after each try, fail the activity after 2 attempts
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

        var saga = new Saga(new List<string>());

        var userId = purchaseModel.UserId;
        var flightsId = purchaseModel.FlightsId;
        var passenger = purchaseModel.Passenger;

        try
        {
            //availability check

            foreach (var flightId in flightsId)
            {
                try
                {
                    var isFlightExists =
                        await Workflow.ExecuteActivityAsync(
                            (PurchaseActivities act) => act.IsFlightExistsAsync(flightId), options);

                    if (!isFlightExists) throw new ApplicationException("Flight does not exist.");

                    var handle = Workflow.GetExternalWorkflowHandle<FlightWorkflow>(flightId);

                    //handle.SignalAsync();
                    //cant use query ????)))))))
            }
                catch (RpcException ex)
                {
                    if (ex.Code is not RpcException.StatusCode.NotFound) throw;

                    //flight workflow was not started yet so we are going to create it
                }

            }

            var isFlightsAvailable =
                await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                    act.IsFlightsAvailableAsync(flightsId), options);

            if (!isFlightsAvailable) throw new ApplicationException("Requested flights are not available.");

            //there is no compensation for ticket availability check
            //lock tickets activity

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.BookFlightAsync(userId, flightsId, passenger), options);

            saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.BookFlightCompensationAsync(userId, flightsId, passenger), options));

            //wait 15 min to pay for ticket

            var isPaid = await Workflow.WaitConditionAsync(() => _isPaid, TimeSpan.FromMinutes(15));

            if (!isPaid) throw new ApplicationException("Tickets was not paid in 15 min.");

            //hold money activity

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyAsync(userId, flightsId), options);

            saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyCompensationAsync(userId, flightsId), options));

            //confirmation activity

            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.ConfirmPurchaseAsync(userId, flightsId, passenger), options);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.ConfirmPurchaseCompensationAsync(userId, flightsId, passenger),
                    options));

            //generate tickets

            var blobTickets = await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GenerateTicketsAsync(userId, flightsId, passenger), options);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.GenerateTicketsCompensationAsync(blobTickets), options));

            //sending tickets

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SendTicketsAsync(userId, blobTickets), options);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.SendTicketsCompensationAsync(userId, blobTickets), options));

            //save info (ticket) to db

            var tickets = await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SaveTicketsAsync(blobTickets), options);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.SaveTicketsCompensationAsync(tickets), options));

            //confirm withdraw
            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.ConfirmWithdrawAsync(userId, flightsId),
                options);

            //possible cancellation
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
        catch (Exception ex)
        {
            saga.OnCompensationError(async (log) =>
            {
                log.Add("Compensation error. Manual intervention required!");
            });

            saga.OnCompensationComplete(async (log) =>
            {
                log.Add("Compensation completed successfully");
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
