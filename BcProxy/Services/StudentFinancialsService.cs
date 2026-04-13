using BcProxy.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BcProxy.Services;

public class StudentFinancialsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StudentFinancialsService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _studentfeesEntity;
    private readonly string _customerBalanceEntity;

    public StudentFinancialsService(
        HttpClient httpClient,
        ILogger<StudentFinancialsService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _studentfeesEntity = configuration["BusinessCentral:StudentfeesEntity"]
            ?? throw new InvalidOperationException("BusinessCentral:StudentfeesEntity is not configured");
        _customerBalanceEntity = configuration["BusinessCentral:CustomerBalanceEntity"]
            ?? throw new InvalidOperationException("BusinessCentral:CustomerBalanceEntity is not configured");
    }

    /// <summary>
    /// Returns all Grade 10 students with their fee breakdown and current balance for a given term.
    /// </summary>
    public async Task<List<Grade10StudentFinancials>> GetGrade10FinancialsAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching Grade 10 financials for term: {Term}", term);

        // 1. Get all Studentfees lines for GRADE 10 + the requested term
        var feeLines = await GetGrade10FeeLines(term, cancellationToken);
        _logger.LogDebug("Retrieved {Count} fee lines for GRADE 10 term {Term}", feeLines.Count, term);

        if (feeLines.Count == 0)
        {
            _logger.LogWarning("No Studentfees records found for GRADE 10 term {Term}", term);
            return new List<Grade10StudentFinancials>();
        }

        // 2. Get all student balances from customerbalance
        var balances = await GetAllBalancesAsync(cancellationToken);
        var balanceDict = balances
            .Where(b => !string.IsNullOrEmpty(b.StudentNo))
            .ToDictionary(b => b.StudentNo!, StringComparer.OrdinalIgnoreCase);
        _logger.LogDebug("Retrieved {Count} customer balance records", balances.Count);

        // 3. Group fee lines by student and build the response
        var grouped = feeLines
            .Where(f => !string.IsNullOrEmpty(f.StudentNo))
            .GroupBy(f => f.StudentNo!, StringComparer.OrdinalIgnoreCase);

        var result = new List<Grade10StudentFinancials>();

        foreach (var studentGroup in grouped)
        {
            var studentNo = studentGroup.Key;
            var lines = studentGroup.ToList();
            var firstLine = lines.First();

            balanceDict.TryGetValue(studentNo, out var balance);

            var studentFinancials = new Grade10StudentFinancials
            {
                StudentNo = studentNo,
                Name = balance?.Name ?? string.Empty,
                Phone = balance?.PhoneNo ?? string.Empty,
                Programme = firstLine.Programme ?? string.Empty,
                Term = firstLine.Term ?? string.Empty,
                CurrentBalance = balance?.BalanceLcy ?? 0,
                Fees = lines.Select(l => new FeeLineItem
                {
                    FeeCode = l.FeeItem ?? string.Empty,
                    Description = l.Description ?? string.Empty,
                    Amount = l.Amount
                }).ToList()
            };

            studentFinancials.TermTotal = studentFinancials.Fees.Sum(f => f.Amount);
            result.Add(studentFinancials);
        }

        _logger.LogInformation("Returning financials for {Count} Grade 10 students", result.Count);
        return result.OrderBy(s => s.StudentNo).ToList();
    }

    /// <summary>
    /// Returns financials for a single student by their student number.
    /// </summary>
    public async Task<Grade10StudentFinancials?> GetStudentFinancialsAsync(
        string studentNo,
        string term,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching financials for student {StudentNo} term {Term}", studentNo, term);

        // 1. Get fee lines for this specific student and term
        var filter = $"$filter=Student_No eq '{studentNo}' and Term eq '{term}'";
        var response = await _httpClient.GetAsync($"{_studentfeesEntity}?{filter}", cancellationToken);
        await EnsureSuccessAsync(response);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var odataResult = JsonSerializer.Deserialize<ODataResponse<StudentFee>>(content, _jsonOptions);
        var feeLines = odataResult?.Value ?? new List<StudentFee>();

        if (feeLines.Count == 0)
        {
            _logger.LogWarning("No fee records found for student {StudentNo} term {Term}", studentNo, term);
            return null;
        }

        // 2. Get this student's balance
        var balanceResponse = await _httpClient.GetAsync(
            $"{_customerBalanceEntity}?$filter=No eq '{studentNo}'", cancellationToken);
        await EnsureSuccessAsync(balanceResponse);

        var balanceContent = await balanceResponse.Content.ReadAsStringAsync(cancellationToken);
        var balanceResult = JsonSerializer.Deserialize<ODataResponse<CustomerBalance>>(balanceContent, _jsonOptions);
        var balance = balanceResult?.Value?.FirstOrDefault();

        var firstLine = feeLines.First();

        var financials = new Grade10StudentFinancials
        {
            StudentNo = studentNo,
            Name = balance?.Name ?? string.Empty,
            Phone = balance?.PhoneNo ?? string.Empty,
            Programme = firstLine.Programme ?? string.Empty,
            Term = firstLine.Term ?? string.Empty,
            CurrentBalance = balance?.BalanceLcy ?? 0,
            Fees = feeLines.Select(l => new FeeLineItem
            {
                FeeCode = l.FeeItem ?? string.Empty,
                Description = l.Description ?? string.Empty,
                Amount = l.Amount
            }).ToList()
        };

        financials.TermTotal = financials.Fees.Sum(f => f.Amount);
        return financials;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    private async Task<List<StudentFee>> GetGrade10FeeLines(string term, CancellationToken cancellationToken)
    {
        var filter = $"$filter=Module eq 'GRADE 10' and Term eq '{term}'";
        var response = await _httpClient.GetAsync($"{_studentfeesEntity}?{filter}", cancellationToken);
        await EnsureSuccessAsync(response);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var odataResult = JsonSerializer.Deserialize<ODataResponse<StudentFee>>(content, _jsonOptions);
        return odataResult?.Value ?? new List<StudentFee>();
    }

    private async Task<List<CustomerBalance>> GetAllBalancesAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(_customerBalanceEntity, cancellationToken);
        await EnsureSuccessAsync(response);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var odataResult = JsonSerializer.Deserialize<ODataResponse<CustomerBalance>>(content, _jsonOptions);
        return odataResult?.Value ?? new List<CustomerBalance>();
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Business Central returned {StatusCode}: {Error}", response.StatusCode, errorContent);

            throw new BusinessCentralException(
                $"Business Central API returned {response.StatusCode}: {errorContent}",
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
