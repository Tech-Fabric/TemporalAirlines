using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Services.Interfaces.Flight;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Web.ViewComponents;

public class FlightBookingFormViewComponent : ViewComponent
{
    private readonly Dictionary<string, string> _airports = new()
    {
        { "LAX", "Los Angeles International Airport" },
        { "JFK", "John F. Kennedy International Airport" },
        { "LHR", "London Heathrow Airport" },
        { "HND", "Tokyo International Airport" },
        { "ATL", "Hartsfield Jackson Atlanta International" },
        { "CDG", "Charles de Gaulle Airport" },
        { "FRA", "Frankfurt Airport" },
        { "AMS", "Amsterdam Airport Schiphol" },
        { Airports.ErrorCode, Airports.ErrorName },
        { "IST", "Istanbul Airport" },
        { "MAD", "Madrid Barajas Airport" },
        { "BCN", "Barcelona–El Prat Airport" },
        { "DUB", "Dublin Airport" },
        { "ZRH", "Zurich Airport" },
        { "LIS", "Lisbon Airport" },
        { "FCO", "Fiumicino" },
        { "SVO", "Sheremetyevo" },
    };

    private readonly IFlightService _flightService;

    public FlightBookingFormViewComponent(IFlightService flightService)
    {
        _flightService = flightService;
    }

    public async Task GenerateFlights()
    {
        var columnIdentifiers = new List<string>()
        {
            "A", "B", "C", "D", "E", "F"
        };

        var minutesFlightTime = new[] { 15, 30, 45 };

        for (var i = 0; i < 10; i++)
        {
            var departureFrom = _airports.ElementAt(i).Key;
            var arrivalTo = GetRandomValueExcluding(departureFrom);

            var departureTime = DateTime.UtcNow.AddDays(new Random().Next(90, 180));
            var arrivalTime = departureTime.AddHours(new Random().Next(0, 3)).AddMinutes(minutesFlightTime[new Random().Next(0, 2)]);

            var seatRowsCount = 20;
            var seatColumnsCount = columnIdentifiers.Count;

            var flightToCreate = new FlightInputModel()
            {
                From = departureFrom,
                To = arrivalTo,
                Depart = departureTime,
                Arrival = arrivalTime,
                Seats = []
            };

            for (var x = 0; x < seatRowsCount; x++)
            {
                for (var y = 0; y < seatColumnsCount; y++)
                {
                    flightToCreate.Seats.Add(new SeatInputModel
                    {
                        Name = $"{columnIdentifiers[y]}{x + 1}"
                    });
                }
            }

            await _flightService.CreateFlight(flightToCreate);
        }
    }

    public async Task<IViewComponentResult> InvokeAsync(FlightBookingFormViewModel? model)
    {
        if (model == null)
        {
            model = new FlightBookingFormViewModel();
        }

        if (model.When == null)
        {
            model.When = DateTime.Now;
        }

        model.Airports = _airports;
        model.Flights = await _flightService.GetFlights();

        if (model.Flights.Count == 0)
        {
            await GenerateFlights();
        }

        model.Flights = await _flightService.GetFlights();

        if (!string.IsNullOrEmpty(model.DepartureAirport) && model.DepartureAirport != "From")
        {
            model.Flights = model.Flights.Where(f => f.From == model.DepartureAirport).ToList();
        }

        if (!string.IsNullOrEmpty(model.ArrivalAirport) && model.ArrivalAirport != "To")
        {
            model.Flights = model.Flights.Where(f => f.To == model.ArrivalAirport).ToList();
        }

        return View(model);
    }

    public string GetRandomValueExcluding(string excludedValue)
    {
        var filteredValues = _airports.Keys
            .Where(key => key != excludedValue)
            .ToList();

        int randomIndex = new Random().Next(filteredValues.Count);
        string randomValue = filteredValues[randomIndex];

        return randomValue;
    }
}
