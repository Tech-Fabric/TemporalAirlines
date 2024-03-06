using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.Services.Models.Flight;
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

    private bool _isSeatsReserved;

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

        _isSeatsReserved = true;

        _saga.AddCompensation(async () =>
        {
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.TicketReservationCompensation(seatReservation), _activityOptions);

            _isSeatsReserved = false;
        });
    }

    [WorkflowQuery]
    public string GetFlightId()
    {
        return _flightId;
    }

    [WorkflowQuery]
    public bool IsPaid()
    {
        return _isPaid;
    }

    [WorkflowQuery]
    public bool IsSeatsReserved()
    {
        return _isSeatsReserved;
    }

    [WorkflowRun]
    public async Task<bool> Run(PurchaseModel purchaseModel)
    {
        _flightId = purchaseModel.FlightId.ToString();

        try
        {
            return await ProcessPurchase(purchaseModel);
        }
        catch (Exception)
        {
            _saga.OnCompensationError(log =>
            {
                log.Add("Compensation error. Manual intervention required!");

                return Task.CompletedTask;
            });

            _saga.OnCompensationComplete(log =>
            {
                log.Add("Compensation completed successfully");

                return Task.CompletedTask;
            });

            await _saga.Compensate();

            throw;
        }
    }

    private async Task<bool> ProcessPurchase(PurchaseModel purchaseModel)
    {
        var isFlightsAvailable = await Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.IsFlightAvailable(new FlightAvailabilityModel
            {
                FlightId = purchaseModel.FlightId,
                NumberOfTickets = purchaseModel.NumberOfTickets
            }),
            _activityOptions);

        if (!isFlightsAvailable)
            throw new ApplicationFailureException("Flight is not available for booking.");

        await BookTicketsForFlight(purchaseModel);

        var isPaid = await Workflow.WaitConditionAsync(() => _isPaid, TimeSpan.FromMinutes(15));

        if (!isPaid)
            throw new ApplicationFailureException("Tickets was not paid in 15 min.");

        return await ProceedPayment();
    }

    private async Task<bool> ProceedPayment()
    {
        await HoldMoney();

        await MarkTicketsAsPaid();

        await GenerateBlobTickets();

        await SendTickets();

        await SaveTickets();

        await ConfirmWithdrawal();

        return true;
    }

    private async Task BookTicketsForFlight(PurchaseModel purchaseModel)
    {
        for (var i = 0; i < purchaseModel.NumberOfTickets; i++)
        {
            var ticket = await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.GetTicket(
                    new GetTicketModel
                    {
                        Purchase = purchaseModel,
                        PurchaseId = Workflow.Info.WorkflowId
                    }),
                _activityOptions);

            await BookTicket(ticket);
        }
    }

    private async Task ConfirmWithdrawal()
    {
        await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.ConfirmWithdraw(),
            _activityOptions);

        _saga.AddCompensation(async () =>
            await Workflow.ExecuteActivityAsync(
                (PurchaseActivities act) => act.ConfirmWithdrawCompensation(), _activityOptions));
    }

    private async Task BookTicket(TicketDetailsModel ticket)
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
            await Workflow.ExecuteActivityAsync((PurchaseActivities act) => act.SaveTicketsCompensation(saveSignal),
                _activityOptions));
    }
}