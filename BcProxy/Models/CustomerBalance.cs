using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central customerbalance OData entity.
/// The No field matches Student_No in Studentfees — this is the join key.
/// </summary>
public class CustomerBalance
{
    [JsonPropertyName("No")]
    public string? StudentNo { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("Balance_LCY")]
    public decimal BalanceLcy { get; set; }

    [JsonPropertyName("Balance_Due_LCY")]
    public decimal BalanceDueLcy { get; set; }

    [JsonPropertyName("Sales_LCY")]
    public decimal SalesLcy { get; set; }

    [JsonPropertyName("Payments_LCY")]
    public decimal PaymentsLcy { get; set; }
}
