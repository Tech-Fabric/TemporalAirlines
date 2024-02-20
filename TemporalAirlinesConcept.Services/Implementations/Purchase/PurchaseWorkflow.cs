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
    private Saga _saga = new([]);

    private string _flightId;

    private bool _isPaid;

    private bool _isCancelled;

    private bool _seatsSelected;

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
    public async Task<bool> Run(PurchaseModel purchaseModel)
    {
        _flightId = purchaseModel.FlightId;

        try
        {
            return await ProcessPurchase(purchaseModel);
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

            await _saga.Compensate();

            throw;
        }
    }

    /// <summary>
    /// Sets the paid status to true.
    /// </summary>
    [WorkflowSignal]
    public Task SetAsPaid()
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

    [WorkflowSignal]
    public async Task TicketReservation(PurchaseTicketReservationSignal seatReservation)
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.TicketReservation(seatReservation), _activityOptions);

        _saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.TicketReservationCompensation(seatReservation), _activityOptions));
    }

    private async Task<bool> ProcessPurchase(PurchaseModel purchaseModel)
    {
        var isFlightsAvailable = await Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.IsFlightAvailable(purchaseModel.FlightId),
            _activityOptions);

        if (!isFlightsAvailable)
            return false;

        await BookTicketsForFlight(purchaseModel);

        var isPaid = await Workflow.WaitConditionAsync(() => _isPaid, TimeSpan.FromMinutes(15));

        if (!isPaid)
            throw new ApplicationFailureException("Tickets was not paid in 15 min.");

        return await ProceedPayment(purchaseModel);
    }

    private async Task<bool> ProceedPayment(PurchaseModel purchaseModel)
    {
        await HoldMoney();

        await MarkTicketsAsPaid();

        var flight = await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.GetFlight(purchaseModel.FlightId), _activityOptions);

        await GenerateBlobTickets();

        await SendTickets();

        await SaveTickets();

        await ConfirmWithdrawal();

        var timeUntilDepart = flight.Depart.Subtract(Workflow.UtcNow);
        var isCancelled = await Workflow.WaitConditionAsync(() => _isCancelled, timeUntilDepart);

        if (isCancelled)
            throw new ApplicationFailureException("Purchase has being cancelled");

        return true;
    }

    private async Task BookTicketsForFlight(PurchaseModel purchaseModel)
    {
        for (var i = 0; i < purchaseModel.NumberOfTickets; i++)
        {
            var ticket = await GetTicket(purchaseModel);

            await BookTicket(ticket);
        }
    }

    private static Task<Ticket> GetTicket(PurchaseModel purchaseModel)
    {
        return Task.FromResult(new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            FlightId = purchaseModel.FlightId,
            UserId = purchaseModel.UserId,
            PurchaseId = Workflow.Info.WorkflowId,
            Seat = null,
            PaymentStatus = PaymentStatus.Pending
        });
    }

    private async Task ConfirmWithdrawal()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.ConfirmWithdraw(),
            _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.ConfirmWithdrawCompensation(), _activityOptions));
    }

    private async Task BookTicket(Ticket ticket)
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.BookTicket(ticket),
            _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.BookTicketCompensation(ticket), _activityOptions));
    }

    private async Task HoldMoney()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.HoldMoney(), _activityOptions);

        _saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.HoldMoneyCompensation(), _activityOptions));
    }

    private async Task MarkTicketsAsPaid()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) =>
            act.MarkTicketAsPaid(new MarkTicketPaidSignalModel
            {
                FlightId = _flightId,
                PurchaseId = Workflow.Info.WorkflowId
            }), _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.MarkTicketAsPaidCompensation(new MarkTicketPaidSignalModel
                {
                    FlightId = _flightId,
                    PurchaseId = Workflow.Info.WorkflowId
                }), _activityOptions));
    }

    private async Task GenerateBlobTickets()
    {
        await Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.GenerateBlobTickets(), _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.GenerateBlobTicketsCompensation(), _activityOptions));
    }

    private async Task SendTickets()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SendTickets(), _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.SendTicketsCompensation(), _activityOptions));
    }

    private async Task SaveTickets()
    {
        var saveSignal = new SaveTicketsSignalModel
        {
            FlightId = _flightId,
            PurchaseId = Workflow.Info.WorkflowId
        };

        await Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.SaveTickets(saveSignal),
            _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SaveTicketsCompensation(saveSignal), _activityOptions));
    }
}