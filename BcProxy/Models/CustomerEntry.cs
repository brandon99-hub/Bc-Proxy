using System.Text.Json.Serialization;

namespace BcProxy.Models;

public class CustomerEntry
{
    [JsonPropertyName("Document_No")] 
    public string? DocumentNo { get; set; }

    [JsonPropertyName("Document_Type")] 
    public string? DocumentType { get; set; }

    [JsonPropertyName("Student_No")] 
    public string? StudentNo { get; set; }

    [JsonPropertyName("Description")] 
    public string? Description { get; set; }

    [JsonPropertyName("Posting_Date")] 
    public string? PostingDate { get; set; }

    [JsonPropertyName("Credit_Amount")] 
    public decimal CreditAmount { get; set; }

    [JsonPropertyName("Amount")] 
    public decimal Amount { get; set; }

    [JsonPropertyName("Fee_Item")] 
    public string? FeeItem { get; set; }

    [JsonPropertyName("Reversed")] 
    public bool Reversed { get; set; }
}
