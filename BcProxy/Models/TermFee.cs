using System.Text.Json.Serialization;

namespace BcProxy.Models;

public class TermFee
{
    [JsonPropertyName("Programme")] 
    public string? Programme { get; set; }

    [JsonPropertyName("Module")] 
    public string? Module { get; set; }

    [JsonPropertyName("Term")] 
    public string? Term { get; set; }

    [JsonPropertyName("Fee_Item")] 
    public string? FeeItem { get; set; }

    [JsonPropertyName("Description")] 
    public string? Description { get; set; }

    [JsonPropertyName("Amount")] 
    public decimal Amount { get; set; }
}
