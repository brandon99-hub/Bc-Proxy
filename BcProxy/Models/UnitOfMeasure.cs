using System.Text.Json.Serialization;

namespace BcProxy.Models;

public class UnitOfMeasure
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("International_Standard_Code")]
    public string? InternationalStandardCode { get; set; }
}



