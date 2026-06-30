using BcProxy.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BcProxy.Services;

public class StandardFeesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StandardFeesService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _termfeesEntity;

    public StandardFeesService(
        HttpClient httpClient,
        ILogger<StandardFeesService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _termfeesEntity = configuration["BusinessCentral:TermfeesEntity"]
            ?? throw new InvalidOperationException("BusinessCentral:TermfeesEntity is not configured");
    }

    public async Task<List<TermFee>> GetStandardFeesAsync(
        string? module,
        string? term,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching Standard Fees for module {Module} term {Term}", module, term);

        var filterConditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(module)) filterConditions.Add($"Module eq '{module}'");
        if (!string.IsNullOrWhiteSpace(term)) filterConditions.Add($"Term eq '{term}'");

        var query = filterConditions.Count > 0 ? $"?$filter={string.Join(" and ", filterConditions)}" : "";
        var response = await _httpClient.GetAsync($"{_termfeesEntity}{query}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Business Central returned {StatusCode}: {Error}", response.StatusCode, errorContent);
            throw new BusinessCentralException(
                $"Business Central API returned {response.StatusCode}: {errorContent}",
                response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var odataResult = JsonSerializer.Deserialize<ODataResponse<TermFee>>(content, _jsonOptions);
        return odataResult?.Value ?? new List<TermFee>();
    }

    private class ODataResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? ODataContext { get; set; }

        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }
    }
}
