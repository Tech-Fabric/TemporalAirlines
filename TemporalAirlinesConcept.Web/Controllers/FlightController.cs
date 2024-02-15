﻿using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.DAL.Models.Seat;
using TemporalAirlinesConcept.Services.Implementations.Purchase;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents;

namespace TemporalAirlinesConcept.Web.Controllers;

[Route("flights")]
public class FlightController : Controller
{
    private readonly ITicketService _ticketService;

    public FlightController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("form")]
    public async Task<IActionResult> Form()
    {
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent));
        }
        else
        {
            return View("~/Views/Home/Index.cshtml");
        }
    }

    [HttpGet("{SelectedFlight}")]
    public async Task<IActionResult> Flight([FromRoute] string? selectedFlight)
    {
        FlightBookingFormViewModel model = new FlightBookingFormViewModel();

        return await Flight(model, selectedFlight);
    }

    [HttpPost("{SelectedFlight}")]
    public async Task<IActionResult> Flight([FromForm] FlightBookingFormViewModel model, [FromRoute] string? selectedFlight)
    {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpPost("{SelectedFlight}/purchase")]
    public async Task<IActionResult> Purchase(
        [FromForm] FlightBookingFormViewModel model,
        [FromRoute] string? selectedFlight
    )
    {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        if (!string.IsNullOrEmpty(model.CreditCardDetails.CardNumber))
        {
            model.PurchaseWorkflowId = await _ticketService.RequestTicketPurchase(
                new PurchaseModel()
                {
                    FlightId = model.SelectedFlight,
                    NumberOfTickets = model.NumberOfSeats
                }
            );

            HttpContext.Session.SetInt32("NumberOfSeats", model.NumberOfSeats);

            // TODO: fetch workflow status

            model.PaymentSuccessful = true;
        }

        if (Request.IsHtmx())
        {
            if (!string.IsNullOrEmpty(model.PurchaseWorkflowId))
            {
                Response.Htmx(h =>
                {
                    h.PushUrl($"/flights/{model.SelectedFlight}/ticket/{model.PurchaseWorkflowId}");
                });
            }

            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpGet("{SelectedFlight}/ticket/{PurchaseWorkflowId}")]
    public async Task<IActionResult> GetDetails(
        [FromForm] FlightBookingFormViewModel model,
        [FromRoute] string? selectedFlight,
        [FromRoute] string? purchaseWorkflowId
    )
    {
        model ??= new FlightBookingFormViewModel();

        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        model.PurchaseWorkflowId = purchaseWorkflowId;
        model.PaymentSuccessful = true;

        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpPost("{SelectedFlight}/ticket/{PurchaseWorkflowId}")]
    public async Task<IActionResult> Details(
        [FromForm] FlightBookingFormViewModel model,
        [FromRoute] string? selectedFlight,
        [FromRoute] string? purchaseWorkflowId
    )
    {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        model.NumberOfSeats = HttpContext.Session.GetInt32("NumberOfSeats") ?? default;

        model.PurchaseWorkflowId = purchaseWorkflowId;
        model.PaymentSuccessful = true;

        var seatsList = model.SelectedSeats.Where(s => s.Value)
            .Select(s => s.Key).ToList();

        if (seatsList.Count > model.NumberOfSeats)
        {
            ModelState.AddModelError(nameof(model.NumberOfSeats), "Too many seats selected");
        }

        if (ModelState.IsValid)
        {
            await _ticketService.RequestSeatReservation(new SeatReservationInputModel()
            {
                FlightId = model.SelectedFlight,
                PurchaseId = model.PurchaseWorkflowId,
                Seats = seatsList
            });

            model.IsConfirmed = true;

            //model.PurchaseWorkflowId, model.SelectedSeats.Where(kv => kv.Value).Select(kv => kv.Key).ToList());
        }

        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpPost("{WorkflowId}/payment")]
    public async Task<IActionResult> MarkAsPaid(
        [FromForm] FlightBookingFormViewModel model,
        [FromRoute] string? workflowId)
    {
        if (!string.IsNullOrEmpty(workflowId))
        {
            await _ticketService.MarkAsPaid(workflowId);
            model.PurchaseWorkflowId = workflowId;
            model.PaymentSuccessful = true;
            model.IsConfirmed = true;
        }

        if (Request.IsHtmx())
        {
            Response.Htmx(h =>
            {
                h.PushUrl($"/flights/{model.SelectedFlight}/ticket/{model.PurchaseWorkflowId}");
            });

            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }
}
