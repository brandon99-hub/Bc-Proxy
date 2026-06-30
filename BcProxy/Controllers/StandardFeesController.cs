using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class StandardFeesController : ControllerBase
{
    private readonly StandardFeesService _feesService;
    private readonly ILogger<StandardFeesController> _logger;

    public StandardFeesController(StandardFeesService feesService, ILogger<StandardFeesController> logger)
    {
        _feesService = feesService;
        _logger = logger;
    }

    /// <summary>
    /// Get standard fees optionally filtered by module and term.
    /// </summary>
    /// <param name="module">e.g. "FORM 2"</param>
    /// <param name="term">e.g. "TERM 1"</param>
    [HttpGet]
    public async Task<ActionResult<List<TermFee>>> GetStandardFees(
        [FromQuery] string? module,
        [FromQuery] string? term,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /standardfees?module={Module}&term={Term}", module, term);

            var result = await _feesService.GetStandardFeesAsync(module?.Trim().ToUpper(), term?.Trim().ToUpper(), cancellationToken);

            return Ok(result);
        }
        catch (BusinessCentralException ex)
        {
            _logger.LogError(ex, "BC error fetching standard fees");
            return ex.StatusCode.HasValue
                ? StatusCode((int)ex.StatusCode.Value, new { error = "Business Central Error", message = ex.Message })
                : StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching standard fees");
            return StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout fetching standard fees");
            return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching standard fees");
            return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }
}
