namespace Inventory.Domain.Entities;

public class Warehouse
{
    private readonly List<StockItem> _stockItems = [];

    private Warehouse()
    {
    }

    public Warehouse(string name, string location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Location = location;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    public IReadOnlyCollection<StockItem> StockItems => _stockItems;

    public StockItem AddStock(string sku, int quantity)
    {
        var item = new StockItem(sku, quantity, Id);
        _stockItems.Add(item);
        return item;
    }
}
