namespace BcProxy.Models;

public class ItemDetails
{
    public string? No { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Inventory { get; set; }

    public List<BaseUnitDetails> BaseUnits { get; set; } = new();

    public List<ItemUnitOfMeasure> PurchaseUnits { get; set; } = new();
}

public class BaseUnitDetails
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? InternationalStandardCode { get; set; }
}

