using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

[Workflow]
public class PurchaseWorkflow
{
    private List<Ticket> _tickets = [];
    
    private Saga _saga = new([]);
    
    private bool _isPaid;

    private bool _isCancelled;

    private readonly ActivityOptions _activityOptions = new()
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
        try
        {
            return await PurchaseWorkflowBodyAsync(purchaseModel);
        }
        catch (Exception)
        {
            _saga.OnCompensationError((log) =>
            {
                log.Add("Compensation error. Manual intervention required!");
                
                return Task.CompletedTask;
            });

            _saga.OnCompensationComplete((log) =>
            {
                log.Add("Compensation completed successfully");
                
                return Task.CompletedTask;
            });

            await _saga.CompensateAsync();

            throw;
        }
    }

    private async Task<bool> PurchaseWorkflowBodyAsync(PurchaseModel purchaseModel)
    {
        var isFlightsAvailable = await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.IsFlightsAvailableAsync(purchaseModel.FlightsId), _activityOptions);

        if (!isFlightsAvailable) 
                return false;

        await BookTicketsForFlightAsync(purchaseModel);
            
        var isPaid = await Workflow.WaitConditionAsync(() => _isPaid, TimeSpan.FromMinutes(15));

        if (!isPaid) 
            throw new ApplicationFailureException("Tickets was not paid in 15 min.");

        return await ProceedPaymentAsync(purchaseModel);
    }
    
    private async Task<bool> ProceedPaymentAsync(PurchaseModel purchaseModel)
    {
        await HoldMoneyAsync();

        foreach (var ticket in _tickets)
            await MarkTicketPaidAsync(ticket);

        await GenerateBlobTicketsAsync();

        await SendTicketsAsync();

        await SaveTicketsAsync();
            
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
    
    private async Task BookTicketsForFlightAsync(PurchaseModel purchaseModel)
    {
        foreach (var flightId in purchaseModel.FlightsId)
        {
            var ticket = await FormTicketAsync(flightId, purchaseModel);

            await CreateTicketAsync(ticket);            

            _tickets.Add(ticket);
        }
    }

    private static Task<Ticket> FormTicketAsync(string flightId, PurchaseModel purchaseModel)
    {
        return Task.FromResult(new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            FlightId = flightId,
            Passenger = purchaseModel.Passenger,
            UserId = purchaseModel.UserId,
            Seat = null,
            PaymentStatus = PaymentStatus.Pending
        });
    }

    private async Task CreateTicketAsync(Ticket ticket)
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.CreateTicketAsync(ticket),
            _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.CreateTicketCompensationAsync(ticket), _activityOptions));
    }
    
    private async Task HoldMoneyAsync()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.HoldMoneyAsync(), _activityOptions);

        _saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.HoldMoneyCompensationAsync(), _activityOptions));
    }
    
    private async Task MarkTicketPaidAsync(Ticket ticket)
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.MarkTicketPaidAsync(ticket),
            _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.MarkTicketPaidCompensationAsync(ticket), _activityOptions));
    }

    private async Task GenerateBlobTicketsAsync()
    {
        await Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.GenerateBlobTicketsAsync(), _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GenerateBlobTicketsCompensationAsync(), _activityOptions));
    }

    private async Task SendTicketsAsync()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SendTicketsAsync(), _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.SendTicketsCompensationAsync(), _activityOptions));
    }

    private async Task SaveTicketsAsync()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SaveTicketsAsync(_tickets),
            _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.SaveTicketsCompensationAsync(_tickets), _activityOptions));
    }
    
    /// <summary>
    /// Sets the paid status to true.
    /// </summary>
    [WorkflowSignal]
    public Task SetPaidStatus()
    {
        if (!_isPaid) 
            _isPaid = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cancels the current workflow.
    /// </summary>
    [WorkflowSignal]
    public Task Cancel()
    {
        if (!_isCancelled) 
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
