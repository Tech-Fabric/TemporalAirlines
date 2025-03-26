using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Models.Assistant;
using TemporalAirlinesConcept.Services.Interfaces.Assistant;

namespace TemporalAirlinesConcept.Api.Controllers;

/// <summary>
/// Controller for handling assistant-related requests.
/// </summary>
[Route("api/assistant")]
[ApiController]
public class AssistantController : ControllerBase
{
    private readonly IAssistantService _assistantService;

    public AssistantController(IAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    /// <summary>
    /// Processes the user request.
    /// </summary>
    /// <param name="request">The request to the assistant.</param>
    /// <returns>The result of processing the user request.</returns>
    [HttpPost("process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessUserRequest([FromBody] RequestToAssistant request)
    {
        var result = await _assistantService.ProcessUserRequest(request);

        return Ok(result);
    }
}
