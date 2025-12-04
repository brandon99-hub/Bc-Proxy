using BcProxy.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BcProxy.Services;

public class BusinessCentralService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BusinessCentralService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _unitOfMeasureEntity;
    private readonly string _itemUnitOfMeasureEntity;

    public BusinessCentralService(
        HttpClient httpClient, 
        ILogger<BusinessCentralService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        _unitOfMeasureEntity = configuration["BusinessCentral:UnitOfMeasureEntity"] 
            ?? throw new InvalidOperationException("BusinessCentral:UnitOfMeasureEntity is not configured in appsettings.json");
        _itemUnitOfMeasureEntity = configuration["BusinessCentral:ItemUnitOfMeasureEntity"] 
            ?? throw new InvalidOperationException("BusinessCentral:ItemUnitOfMeasureEntity is not configured in appsettings.json");
    }

    public async Task<ItemDetails?> GetItemDetailsAsync(string itemNo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching item details for {ItemNo} from Business Central", itemNo);

            var encodedNo = Uri.EscapeDataString(itemNo);

            // 1) Get main item
            var itemResponse = await _httpClient.GetAsync($"items('{encodedNo}')", cancellationToken);
            if (itemResponse.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Item {ItemNo} not found in Business Central", itemNo);
                return null;
            }

            await EnsureSuccessStatusCodeAsync(itemResponse);
            var itemContent = await itemResponse.Content.ReadAsStringAsync(cancellationToken);
            var item = JsonSerializer.Deserialize<Item>(itemContent, _jsonOptions);
            if (item == null)
            {
                _logger.LogError("Failed to deserialize Item response for {ItemNo}", itemNo);
                throw new InvalidOperationException("Failed to deserialize Business Central Item response");
            }

            // 2) Get base unit description from UnitsOfMeasure
            UnitOfMeasure? baseUnit = null;
            if (!string.IsNullOrEmpty(item.BaseUnitOfMeasure))
            {
                var baseCode = Uri.EscapeDataString(item.BaseUnitOfMeasure);
                var uomResponse = await _httpClient.GetAsync($"{_unitOfMeasureEntity}?$filter=Code eq '{baseCode}'", cancellationToken);
                await EnsureSuccessStatusCodeAsync(uomResponse);

                var uomContent = await uomResponse.Content.ReadAsStringAsync(cancellationToken);
                var uomList = JsonSerializer.Deserialize<ODataResponse<UnitOfMeasure>>(uomContent, _jsonOptions);
                baseUnit = uomList?.Value?.FirstOrDefault();
            }

            // 3) Get item units of measure for this item
            var itemNoEncodedForFilter = Uri.EscapeDataString(item.No ?? itemNo);
            var iumResponse = await _httpClient.GetAsync($"{_itemUnitOfMeasureEntity}?$filter=Item_No eq '{itemNoEncodedForFilter}'", cancellationToken);
            await EnsureSuccessStatusCodeAsync(iumResponse);

            var iumContent = await iumResponse.Content.ReadAsStringAsync(cancellationToken);
            var iumList = JsonSerializer.Deserialize<ODataResponse<ItemUnitOfMeasure>>(iumContent, _jsonOptions);
            var purchaseUnits = iumList?.Value ?? new List<ItemUnitOfMeasure>();

            var baseUnits = new List<BaseUnitDetails>();
            if (baseUnit != null)
            {
                baseUnits.Add(new BaseUnitDetails
                {
                    Code = item.BaseUnitOfMeasure,
                    Description = baseUnit.Description,
                    InternationalStandardCode = baseUnit.InternationalStandardCode
                });
            }

            var details = new ItemDetails
            {
                No = item.No,
                Description = item.Description,
                Type = item.Type,
                UnitPrice = item.UnitPrice,
                Inventory = item.Inventory,
                BaseUnits = baseUnits,
                PurchaseUnits = purchaseUnits
            };

            _logger.LogDebug("Successfully retrieved item details for {ItemNo}", itemNo);
            return details;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching item details {ItemNo} from Business Central", itemNo);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("Timeout while fetching item details {ItemNo} from Business Central", itemNo);
            throw new HttpRequestException($"Request to Business Central timed out for item {itemNo}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching item details {ItemNo} from Business Central", itemNo);
            throw;
        }
    }

    public async Task<List<Item>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching all items from Business Central");
            var response = await _httpClient.GetAsync("items", cancellationToken);
            
            await EnsureSuccessStatusCodeAsync(response);
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var odataResponse = JsonSerializer.Deserialize<ODataResponse<Item>>(content, _jsonOptions);
            
            _logger.LogDebug("Successfully retrieved {Count} items", odataResponse?.Value?.Count ?? 0);
            
            return odataResponse?.Value ?? new List<Item>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching items from Business Central");
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("Timeout while fetching items from Business Central");
            throw new HttpRequestException("Request to Business Central timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching items from Business Central");
            throw;
        }
    }

    public async Task<List<ItemDetails>> GetItemDetailsListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching all items with details from Business Central");

            // 1) Fetch all items
            var itemsResponse = await _httpClient.GetAsync("items", cancellationToken);
            await EnsureSuccessStatusCodeAsync(itemsResponse);
            var itemsContent = await itemsResponse.Content.ReadAsStringAsync(cancellationToken);
            var itemsList = JsonSerializer.Deserialize<ODataResponse<Item>>(itemsContent, _jsonOptions);
            var items = itemsList?.Value ?? new List<Item>();

            _logger.LogDebug("Retrieved {Count} items, now fetching unit of measure data", items.Count);

            // 2) Fetch all UnitsOfMeasure and build dictionary by Code
            var uomResponse = await _httpClient.GetAsync(_unitOfMeasureEntity, cancellationToken);
            await EnsureSuccessStatusCodeAsync(uomResponse);
            var uomContent = await uomResponse.Content.ReadAsStringAsync(cancellationToken);
            var uomList = JsonSerializer.Deserialize<ODataResponse<UnitOfMeasure>>(uomContent, _jsonOptions);
            var unitsOfMeasure = uomList?.Value ?? new List<UnitOfMeasure>();
            var uomDictionary = unitsOfMeasure
                .Where(u => !string.IsNullOrEmpty(u.Code))
                .ToDictionary(u => u.Code!, StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("Retrieved {Count} units of measure", unitsOfMeasure.Count);

            // 3) Fetch all ItemUnitsOfMeasure and group by Item_No
            var iumResponse = await _httpClient.GetAsync(_itemUnitOfMeasureEntity, cancellationToken);
            await EnsureSuccessStatusCodeAsync(iumResponse);
            var iumContent = await iumResponse.Content.ReadAsStringAsync(cancellationToken);
            var iumList = JsonSerializer.Deserialize<ODataResponse<ItemUnitOfMeasure>>(iumContent, _jsonOptions);
            var itemUnitsOfMeasure = iumList?.Value ?? new List<ItemUnitOfMeasure>();
            var iumByItemNo = itemUnitsOfMeasure
                .Where(i => !string.IsNullOrEmpty(i.ItemNo))
                .GroupBy(i => i.ItemNo!)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("Retrieved {Count} item units of measure entries", itemUnitsOfMeasure.Count);

            // 4) Join everything together
            var result = new List<ItemDetails>();
            foreach (var item in items)
            {
                var baseUnits = new List<BaseUnitDetails>();
                if (!string.IsNullOrEmpty(item.BaseUnitOfMeasure) && 
                    uomDictionary.TryGetValue(item.BaseUnitOfMeasure, out var baseUnit))
                {
                    baseUnits.Add(new BaseUnitDetails
                    {
                        Code = item.BaseUnitOfMeasure,
                        Description = baseUnit.Description,
                        InternationalStandardCode = baseUnit.InternationalStandardCode
                    });
                }

                var purchaseUnits = iumByItemNo.TryGetValue(item.No ?? string.Empty, out var itemUoms)
                    ? itemUoms
                    : new List<ItemUnitOfMeasure>();

                result.Add(new ItemDetails
                {
                    No = item.No,
                    Description = item.Description,
                    Type = item.Type,
                    UnitPrice = item.UnitPrice,
                    Inventory = item.Inventory,
                    BaseUnits = baseUnits,
                    PurchaseUnits = purchaseUnits
                });
            }

            _logger.LogDebug("Successfully built {Count} item details", result.Count);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching item details list from Business Central");
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("Timeout while fetching item details list from Business Central");
            throw new HttpRequestException("Request to Business Central timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching item details list from Business Central");
            throw;
        }
    }

    public async Task<Item?> GetItemByNoAsync(string itemNo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching item {ItemNo} from Business Central", itemNo);
            
            var encodedNo = Uri.EscapeDataString(itemNo);
            var response = await _httpClient.GetAsync($"items('{encodedNo}')", cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Item {ItemNo} not found in Business Central", itemNo);
                return null;
            }
            
            await EnsureSuccessStatusCodeAsync(response);
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var item = JsonSerializer.Deserialize<Item>(content, _jsonOptions);
            
            _logger.LogDebug("Successfully retrieved item {ItemNo}", itemNo);
            
            return item;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching item {ItemNo} from Business Central", itemNo);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("Timeout while fetching item {ItemNo} from Business Central", itemNo);
            throw new HttpRequestException($"Request to Business Central timed out for item {itemNo}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching item {ItemNo} from Business Central", itemNo);
            throw;
        }
    }

    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Business Central returned error status {StatusCode}: {ErrorContent}", 
                response.StatusCode, errorContent);
            
            throw new BusinessCentralException(
                $"Business Central API returned status code {response.StatusCode}: {errorContent}",
                response.StatusCode);
        }
    }

    private class ODataResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? ODataContext { get; set; }
        
        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }
    }
}

