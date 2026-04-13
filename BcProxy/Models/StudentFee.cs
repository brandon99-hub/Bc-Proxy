using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central Studentfees OData entity.
/// Each row = one fee line charged to one student for a given term.
/// </summary>
public class StudentFee
{
    [JsonPropertyName("No")]
    public string? BillNo { get; set; }

    [JsonPropertyName("Line_No")]
    public int LineNo { get; set; }

    [JsonPropertyName("Student_No")]
    public string? StudentNo { get; set; }

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
