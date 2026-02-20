namespace Marketplace.Database.Entities;

public class InventoryItem : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }
    public string Unit { get; set; } = "unit";
    public decimal CostPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
    public bool TrackInventory { get; set; } = true;

    public User? User { get; set; }
    public Store? Store { get; set; }
    public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
}

public class InventoryMovement : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string Type { get; set; } = "adjustment"; // purchase | sale | adjustment | return | transfer
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }

    public InventoryItem? InventoryItem { get; set; }
}
