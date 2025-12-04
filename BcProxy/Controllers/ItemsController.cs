using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly BusinessCentralService _bcService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(BusinessCentralService bcService, ILogger<ItemsController> logger)
    {
        _bcService = bcService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> GetItems(CancellationToken cancellationToken)
    {
        try
        {
            var items = await _bcService.GetItemsAsync(cancellationToken);
            return Ok(items);
        }
        catch (BusinessCentralException ex)
        {
            _logger.LogError(ex, "Error fetching items from Business Central");
            
            return ex.StatusCode.HasValue
                ? StatusCode((int)ex.StatusCode.Value, new { error = "Business Central API error", message = ex.Message })
                : StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching items from Business Central");
            return StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Request timeout while fetching items");
            return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching items");
            return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }

    [HttpGet("{no}")]
    public async Task<ActionResult<Item>> GetItem(string no, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _bcService.GetItemByNoAsync(no, cancellationToken);
            
            if (item == null)
            {
                return NotFound(new { error = "Not Found", message = $"Item '{no}' not found" });
            }
            
            return Ok(item);
        }
        catch (BusinessCentralException ex)
        {
            _logger.LogError(ex, "Error fetching item {ItemNo} from Business Central", no);
            
            return ex.StatusCode.HasValue
                ? StatusCode((int)ex.StatusCode.Value, new { error = "Business Central API error", message = ex.Message })
                : StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching item {ItemNo} from Business Central", no);
            return StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Request timeout while fetching item {ItemNo}", no);
            return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching item {ItemNo}", no);
            return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }

    [HttpGet("details")]
    public async Task<ActionResult<List<ItemDetails>>> GetItemsDetails(CancellationToken cancellationToken)
    {
        try
        {
            var details = await _bcService.GetItemDetailsListAsync(cancellationToken);
            return Ok(details);
        }
        catch (BusinessCentralException ex)
        {
            _logger.LogError(ex, "Error fetching items details from Business Central");

            return ex.StatusCode.HasValue
                ? StatusCode((int)ex.StatusCode.Value, new { error = "Business Central API error", message = ex.Message })
                : StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching items details from Business Central");
            return StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Request timeout while fetching items details");
            return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching items details");
            return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }

    [HttpGet("{no}/details")]
    public async Task<ActionResult<ItemDetails>> GetItemDetails(string no, CancellationToken cancellationToken)
    {
        try
        {
            var details = await _bcService.GetItemDetailsAsync(no, cancellationToken);

            if (details == null)
            {
                return NotFound(new { error = "Not Found", message = $"Item '{no}' not found" });
            }

            return Ok(details);
        }
        catch (BusinessCentralException ex)
        {
            _logger.LogError(ex, "Error fetching item details {ItemNo} from Business Central", no);

            return ex.StatusCode.HasValue
                ? StatusCode((int)ex.StatusCode.Value, new { error = "Business Central API error", message = ex.Message })
                : StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching item details {ItemNo} from Business Central", no);
            return StatusCode(502, new { error = "Bad Gateway", message = "Failed to communicate with Business Central" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Request timeout while fetching item details {ItemNo}", no);
            return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching item details {ItemNo}", no);
            return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }
}

