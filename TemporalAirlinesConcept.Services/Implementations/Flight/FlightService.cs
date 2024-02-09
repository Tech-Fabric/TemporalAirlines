using AutoMapper;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Interfaces.Flight;
using TemporalAirlinesConcept.Services.Models.Flight;
using Temporalio.Client;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

public class FlightService : IFlightService
{
    private readonly IMapper _mapper;
    private readonly ITemporalClient _temporalClient;
    private readonly IFlightRepository _flightRepository;

    public FlightService(IMapper mapper, ITemporalClient temporalClient, IFlightRepository flightRepository)
    {
        _mapper = mapper;
        _temporalClient = temporalClient;
        _flightRepository = flightRepository;
    }

    public async Task<List<DAL.Entities.Flight>> GetFlights()
    {
        var flights = await _flightRepository.GetFlightsAsync();

        var flightDetails = await Task.WhenAll(flights.Select(FetchFlightDetailFromWorkflow));

        flights = flightDetails.ToList();

        return flights;
    }

    public async Task<DAL.Entities.Flight> GetFlight(string id)
    {
        var flight = await _flightRepository.GetFlightAsync(id);

        var fetchedFlight = await FetchFlightDetailFromWorkflow(flight);

        return fetchedFlight;
    }

    public async Task<DAL.Entities.Flight> FetchFlightDetailFromWorkflow(DAL.Entities.Flight flight)
    {
        if (flight is null)
            throw new EntityNotFoundException("Flight was not found.");

        if (!await WorkflowHandleHelper.IsWorkflowRunning<FlightWorkflow>(_temporalClient, flight.Id))
            return flight;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flight.Id);

        var flightDetails = await handle.QueryAsync(wf => wf.GetFlightDetails());

        return _mapper.Map<DAL.Entities.Flight>(flightDetails);
    }

    public async Task<DAL.Entities.Flight> CreateFlight(FlightInputModel model)
    {
        var flight = _mapper.Map<DAL.Entities.Flight>(model);

        await _flightRepository.AddFlightAsync(flight);

        return flight;
    }

    public async Task RemoveFlight(string id)
    {
        var flight = await _flightRepository.GetFlightAsync(id);

        if (flight is null)
            throw new EntityNotFoundException("Flight was not found.");

        await _flightRepository.DeleteFlightAsync(id);
    }
}