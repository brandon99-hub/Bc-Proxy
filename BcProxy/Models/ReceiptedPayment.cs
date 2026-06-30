using System.Text.Json.Serialization;

namespace BcProxy.Models;

public class ReceiptedPayment
{
    [JsonPropertyName("No")] 
    public string? No { get; set; }

    [JsonPropertyName("Student_No")] 
    public string? StudentNo { get; set; }

    [JsonPropertyName("Description")] 
    public string? Description { get; set; }

    [JsonPropertyName("Posting_Date")] 
    public string? PostingDate { get; set; }

    [JsonPropertyName("Total_Amount")] 
    public decimal TotalAmount { get; set; }
}
