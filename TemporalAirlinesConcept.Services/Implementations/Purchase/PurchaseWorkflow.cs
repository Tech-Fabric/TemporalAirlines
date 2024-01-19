using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

[Workflow]
public class PurchaseWorkflow
{
    private List<Ticket> _tickets = [];
    
    private bool _isPaid = false;

    private bool _isCancelled = false;

    private readonly ActivityOptions _activityOptions = new ActivityOptions
    {
        StartToCloseTimeout = TimeSpan.FromSeconds(60),
        RetryPolicy = new RetryPolicy
        {
            InitialInterval = TimeSpan.FromSeconds(15),
            BackoffCoefficient = 1,
            MaximumAttempts = 2
        }
    };
    
    [WorkflowRun]
    public async Task<bool> RunAsync(PurchaseModel purchaseModel)
    {
        var saga = new Saga([]);

        try
        {
            return await FlightWorkflowBodyAsync(purchaseModel, saga);
        }
        catch (Exception)
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

    private async Task<bool> FlightWorkflowBodyAsync(PurchaseModel purchaseModel, Saga saga)
    {
        var isFlightsAvailable = await Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.IsFlightsAvailableAsync(purchaseModel.FlightsId), _activityOptions);

            if (!isFlightsAvailable) 
                return false;
            
            foreach (var flightId in purchaseModel.FlightsId)
                _tickets.Add(await BookTicketForFlightAsync(flightId, purchaseModel, saga));
            
            var isPaid = await Workflow.WaitConditionAsync(() => _isPaid, TimeSpan.FromMinutes(15));

            if (!isPaid) 
                throw new ApplicationFailureException("Tickets was not paid in 15 min.");

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyAsync(), _activityOptions);

            saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
                act.HoldMoneyCompensationAsync(), _activityOptions));

            foreach (var ticket in _tickets)
            {
                await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.MarkTicketPaidAsync(ticket),
                    _activityOptions);

                saga.AddCompensation(async () =>
                    await Workflow.ExecuteActivityAsync(
                        (PurchaseActivities act) => act.MarkTicketPaidCompensationAsync(ticket), _activityOptions));
            }

            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GenerateBlobTicketsAsync(), _activityOptions);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.GenerateBlobTicketsCompensationAsync(), _activityOptions));

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SendTicketsAsync(), _activityOptions);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.SendTicketsCompensationAsync(), _activityOptions));

            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SaveTicketsAsync(_tickets),
                _activityOptions);

            saga.AddCompensation(async () =>
                await Workflow.ExecuteActivityAsync(
                    (PurchaseActivities act) => act.SaveTicketsCompensationAsync(_tickets), _activityOptions));
            
            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.ConfirmWithdrawAsync(),
                _activityOptions);
            
            var lastFlight = await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GetLastFlightAsync(purchaseModel.FlightsId), _activityOptions);

            var sleepDuration = lastFlight.Depart.Subtract(Workflow.UtcNow);

            var isCancelled = await Workflow.WaitConditionAsync(() => _isCancelled, sleepDuration);

            if (isCancelled)
                throw new ApplicationFailureException("Purchase has being cancelled");
            
            return true;
    }

    private async Task<Ticket> BookTicketForFlightAsync(string flightId, PurchaseModel purchaseModel, Saga saga)
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

        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.CreateTicketAsync(ticket),
            _activityOptions);

        saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.CreateTicketCompensationAsync(ticket), _activityOptions));

        return ticket;
    }

    /// <summary>
    /// Sets the paid status to true.
    /// </summary>
    [WorkflowSignal]
    public Task SetPaidStatus()
    {
        if (_isPaid is not true) 
            _isPaid = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cancels the current workflow.
    /// </summary>
    [WorkflowSignal]
    public Task Cancel()
    {
        if (_isCancelled is not true) 
            _isCancelled = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a list of tickets.
    /// </summary>
    /// <returns>A List of Ticket objects.</returns>
    [WorkflowQuery]
    public List<Ticket> GetTickets()
    {
        return _tickets;
    }
}
