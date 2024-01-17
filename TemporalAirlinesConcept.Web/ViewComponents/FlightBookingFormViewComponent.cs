using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Flight;

namespace TemporalAirlinesConcept.Web.ViewComponents;

public class FlightBookingFormViewComponent : ViewComponent
{
    private readonly Dictionary<string, string> _airports = new Dictionary<string, string>()
    {
        { "LAX" ,"Los Angeles International Airport" },
        { "JFK" ,"John F. Kennedy International Airport" },
        { "LHR" ,"London Heathrow Airport" },
        { "HND" ,"Tokyo International Airport" },
        { "ATL" ,"Hartsfield Jackson Atlanta International" },
        { "CDG" ,"Charles de Gaulle Airport" },
        { "FRA" ,"Frankfurt Airport" },
        { "AMS" ,"Amsterdam Airport Schiphol" },
        { "IST" ,"Istanbul Airport" },
        { "MAD" ,"Madrid Barajas Airport" },
        { "BCN" ,"Barcelona–El Prat Airport" },
        { "DUB" ,"Dublin Airport" },
        { "ZRH" ,"Zurich Airport" },
        { "LIS" ,"Lisbon Airport" },
        { "FCO" ,"Fiumicino" },
        { "SVO" ,"Sheremetyevo" },
    };
    //
    // private readonly List<FlightViewModel> _flights = new List<FlightViewModel>()
    // {
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Amsterdam Airport Schiphol", ArrivalAirport = "Charles De Gaulle Airport", DepartureTime = DateTime.Now.AddHours(2),
    //         ArrivalTime = DateTime.Now.AddHours(4), Airline = "KLM", FlightNumber = "KL1234"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Heathrow Airport", ArrivalAirport = "Frankfurt Airport", DepartureTime = DateTime.Now.AddHours(3),
    //         ArrivalTime = DateTime.Now.AddHours(5), Airline = "Lufthansa", FlightNumber = "LH5678"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Istanbul Airport", ArrivalAirport = "Zurich Airport", DepartureTime = DateTime.Now.AddHours(3),
    //         ArrivalTime = DateTime.Now.AddHours(6), Airline = "Turkish Airlines", FlightNumber = "TK9101"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Madrid Barajas Airport", ArrivalAirport = "Lisbon Airport", DepartureTime = DateTime.Now.AddHours(1),
    //         ArrivalTime = DateTime.Now.AddHours(3), Airline = "Iberia", FlightNumber = "IB7412"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Dublin Airport", ArrivalAirport = "Heathrow Airport", DepartureTime = DateTime.Now.AddHours(2),
    //         ArrivalTime = DateTime.Now.AddHours(4), Airline = "British Airways", FlightNumber = "BA8833"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Barcelona-El Prat Airport", ArrivalAirport = "Frankfurt Airport", DepartureTime = DateTime.Now.AddHours(3),
    //         ArrivalTime = DateTime.Now.AddHours(5), Airline = "Lufthansa", FlightNumber = "LH3456"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Charles De Gaulle Airport", ArrivalAirport = "Madrid Barajas Airport", DepartureTime = DateTime.Now.AddHours(1),
    //         ArrivalTime = DateTime.Now.AddHours(3), Airline = "Air France", FlightNumber = "AF5621"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Zurich Airport", ArrivalAirport = "Amsterdam Airport Schiphol", DepartureTime = DateTime.Now.AddHours(2),
    //         ArrivalTime = DateTime.Now.AddHours(4), Airline = "KLM", FlightNumber = "KL6449"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Lisbon Airport", ArrivalAirport = "Istanbul Airport", DepartureTime = DateTime.Now.AddHours(3),
    //         ArrivalTime = DateTime.Now.AddHours(7), Airline = "Turkish Airlines", FlightNumber = "TK2825"
    //     },
    //     new FlightViewModel
    //     {
    //         DepartureAirport = "Frankfurt Airport", ArrivalAirport = "Dublin Airport", DepartureTime = DateTime.Now.AddHours(1),
    //         ArrivalTime = DateTime.Now.AddHours(2), Airline = "Ryanair", FlightNumber = "FR4332"
    //     },
    // };

    private readonly IFlightService _flightService;

    public FlightBookingFormViewComponent(IFlightService flightService)
    {
        _flightService = flightService;
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
        model.Flights = await _flightService.GetFlightsAsync();

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
}
