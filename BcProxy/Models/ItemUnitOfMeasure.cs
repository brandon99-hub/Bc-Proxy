using System.Text.Json.Serialization;

namespace BcProxy.Models;

public class ItemUnitOfMeasure
{
    [JsonPropertyName("Item_No")]
    public string? ItemNo { get; set; }

    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Qty_per_Unit_of_Measure")]
    public decimal? QtyPerUnitOfMeasure { get; set; }

    [JsonPropertyName("Height")]
    public decimal? Height { get; set; }

    [JsonPropertyName("Width")]
    public decimal? Width { get; set; }

    [JsonPropertyName("Length")]
    public decimal? Length { get; set; }

    [JsonPropertyName("Cubage")]
    public decimal? Cubage { get; set; }

    [JsonPropertyName("Weight")]
    public decimal? Weight { get; set; }
}



