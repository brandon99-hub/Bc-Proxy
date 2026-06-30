using System.Text.Json.Serialization;

namespace BcProxy.Models;

public class CreditNote
{
    [JsonPropertyName("No")] 
    public string? No { get; set; }

    [JsonPropertyName("Account_No")] 
    public string? StudentNo { get; set; }

    [JsonPropertyName("Applies_to_Doc_No")] 
    public string? AppliesToDocNo { get; set; }

    [JsonPropertyName("Amount")] 
    public decimal Amount { get; set; }

    [JsonPropertyName("Fee_Item")] 
    public string? FeeItem { get; set; }

    [JsonPropertyName("Posting_Date")] 
    public string? PostingDate { get; set; }
}
