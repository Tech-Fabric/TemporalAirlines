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

    public async Task<List<DAL.Entities.Flight>> GetFlightsAsync()
    {
        var flights = await _flightRepository.GetFlightsAsync();

        for (var i = 0; i < flights.Count; i++)
        {
            flights[i] = await GetFlightAsync(flights[i].Id);
        }

        return flights;
    }

    public async Task<DAL.Entities.Flight> GetFlightAsync(string id)
    {
        var flight = await _flightRepository.GetFlightAsync(id);

        if (flight is null)
            throw new EntityNotFoundException("Flight was not found.");

        if (!await WorkflowHadleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, id))
            return flight;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(id);

        var status = await handle.QueryAsync(wf => wf.GetStatus());
        var seats = await handle.QueryAsync(wf => wf.SeatsDetails());
        var registered = await handle.QueryAsync(wf => wf.RegisteredPassengers());
        var boarded = await handle.QueryAsync(wf => wf.BoardedPassengers());

        var registeredIds = registered.Select(ticket => ticket.Id).ToList();

        flight.Status = status;
        flight.Seats = seats;
        flight.Boarded = boarded;
        flight.Registered = registeredIds;

        return flight;
    }

    public async Task<DAL.Entities.Flight> CreateFlightAsync(FlightInputModel model)
    {
        var flight = _mapper.Map<DAL.Entities.Flight>(model);

        await _flightRepository.AddFlightAsync(flight);

        return flight;
    }

    public async Task RemoveFlightAsync(string id)
    {
        var flight = await _flightRepository.GetFlightAsync(id);

        if (flight is null)
            throw new EntityNotFoundException("Flight was not found.");

        await _flightRepository.DeleteFlightAsync(id);
    }
}