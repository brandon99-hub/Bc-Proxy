using System.Text.Json.Serialization;

namespace BcProxy.Models;

public class Item
{
    [JsonPropertyName("No")]
    public string? No { get; set; }
    
    [JsonPropertyName("Description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("Type")]
    public string? Type { get; set; }
    
    // Business Central OData field is typically named Unit_Price on NAV.Item
    [JsonPropertyName("Unit_Price")]
    public decimal? UnitPrice { get; set; }
    
    // Inventory value comes from InventoryField on NAV.Item
    [JsonPropertyName("InventoryField")]
    public decimal? Inventory { get; set; }
    
    // Business Central OData field is typically named Base_Unit_of_Measure on NAV.Item
    [JsonPropertyName("Base_Unit_of_Measure")]
    public string? BaseUnitOfMeasure { get; set; }
}

