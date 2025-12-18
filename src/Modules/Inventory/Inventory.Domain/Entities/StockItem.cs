namespace Inventory.Domain.Entities;

public class StockItem
{
    private StockItem()
    {
    }

    public StockItem(string sku, int quantity, Guid warehouseId)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU is required", nameof(sku));
        }

        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        Id = Guid.NewGuid();
        Sku = sku;
        Quantity = quantity;
        WarehouseId = warehouseId;
    }

    public Guid Id { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public Guid WarehouseId { get; private set; }

    public int ReservedQuantity { get; private set; }

    public void Reserve(int quantity)
    {
        if (quantity <= 0 || quantity > Quantity - ReservedQuantity)
        {
            throw new InvalidOperationException("Invalid reservation amount");
        }

        ReservedQuantity += quantity;
    }
}
