using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController : ControllerBase
{
    private readonly StudentFinancialsService _financialsService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(StudentFinancialsService financialsService, ILogger<StudentsController> logger)
    {
        _financialsService = financialsService;
        _logger = logger;
    }

    /// <summary>
    /// Get financial data for ALL Grade 10 students for a given term.
    /// </summary>
    /// <param name="term">Term name e.g. "TERM 1", "TERM 2", "TERM 3"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of Grade 10 students with fee breakdown and balance</returns>
    [HttpGet("grade10")]
    public async Task<ActionResult<List<Grade10StudentFinancials>>> GetGrade10Financials(
        [FromQuery] string term,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest(new { error = "Bad Request", message = "Query parameter 'term' is required. E.g. ?term=TERM 1" });
        }

        try
        {
            _logger.LogInformation("GET /students/grade10?term={Term}", term);

            var result = await _financialsService.GetGrade10FinancialsAsync(term.Trim().ToUpper(), cancellationToken);

            if (result.Count == 0)
            {
                return NotFound(new { error = "Not Found", message = $"No Grade 10 students found for term '{term}'" });
            }

            return Ok(result);
        }
        catch (BusinessCentralException ex)
        {
            _logger.LogError(ex, "BC error fetching Grade 10 financials for term {Term}", term);
            return ex.StatusCode.HasValue
                ? StatusCode((int)ex.StatusCode.Value, new { error = "Business Central Error", message = ex.Message })
                : StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching Grade 10 financials for term {Term}", term);
            return StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout fetching Grade 10 financials for term {Term}", term);
            return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching Grade 10 financials for term {Term}", term);
            return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Get financial data for a single student by student number.
    /// </summary>
    /// <param name="studentNo">Student number e.g. "3287"</param>
    /// <param name="term">Term name e.g. "TERM 1"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Student financials with fee breakdown and balance</returns>
    [HttpGet("{studentNo}/financials")]
    public async Task<ActionResult<Grade10StudentFinancials>> GetStudentFinancials(
        string studentNo,
        [FromQuery] string term,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest(new { error = "Bad Request", message = "Query parameter 'term' is required. E.g. ?term=TERM 1" });
        }

        try
        {
            _logger.LogInformation("GET /students/{StudentNo}/financials?term={Term}", studentNo, term);

            var result = await _financialsService.GetStudentFinancialsAsync(
                studentNo, term.Trim().ToUpper(), cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = "Not Found", message = $"No records found for student '{studentNo}' in term '{term}'" });
            }

            return Ok(result);
        }
        catch (BusinessCentralException ex)
        {
            _logger.LogError(ex, "BC error fetching financials for student {StudentNo}", studentNo);
            return ex.StatusCode.HasValue
                ? StatusCode((int)ex.StatusCode.Value, new { error = "Business Central Error", message = ex.Message })
                : StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching financials for student {StudentNo}", studentNo);
            return StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout fetching financials for student {StudentNo}", studentNo);
            return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching financials for student {StudentNo}", studentNo);
            return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }
}
