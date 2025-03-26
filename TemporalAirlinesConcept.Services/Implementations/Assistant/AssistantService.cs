using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text;
using TemporalAirlinesConcept.Services.Interfaces.Assistant;
using TemporalAirlinesConcept.Services.Models.Assistant;
using TemporalAirlinesConcept.Services.Interfaces.Flight;

namespace TemporalAirlinesConcept.Services.Implementations.Assistant;

public class AssistantService : IAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IFlightService _flightService;

    public AssistantService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<AssistantAnswer> ProcessUserRequest(RequestToAssistant request)
    {
        string sessionId = request.SessionId ?? Guid.NewGuid().ToString();
        string sessionKey = $"session:{sessionId}";

        if (!_cache.TryGetValue(sessionKey, out List<string> history))
        {
            history = new List<string>();
        }

        history.Add(request.Text);

        var flights = await _flightService.GetFlights();


        var ticketsJson = JsonSerializer.Serialize(flights);

        // Build conversation context for Llama 3
        string conversationContext = string.Join("\n", history);
        string prompt = $@"
                            You are a smart ticket booking assistant. The user is searching for flight tickets.
                            Use the API response below to provide a relevant answer.
                            ---
                            Conversation history:
                            {conversationContext}
                            ---
                              Here is the ticket data: {ticketsJson}
                            Suggest the best options for the user.
                            ";

        var ollamaRequest = new
        {
            model = "llama3",
            prompt = prompt
        };

        var llmResponse = await _httpClient.PostAsync("http://localhost:11434/api/generate",
            new StringContent(JsonSerializer.Serialize(ollamaRequest), Encoding.UTF8, "application/json"));
        var content = llmResponse.IsSuccessStatusCode ? await llmResponse.Content.ReadAsStringAsync() : "Error processing AI response.";

        history.Add($"AI Response: {content}");
        _cache.Set(sessionKey, history, TimeSpan.FromMinutes(30));

        return new AssistantAnswer()
        {
            SessionId = sessionId,
            Message = content
        };
    }
}
