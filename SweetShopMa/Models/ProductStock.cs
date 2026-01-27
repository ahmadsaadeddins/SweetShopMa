using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Location-specific stock level for a product.
/// Composite logic is handled by the unique index on ProductId and LocationId.
/// </summary>
public class ProductStock
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Name = "IdxProductLocation", Order = 1, Unique = true)]
    public int ProductId { get; set; }

    [Indexed(Name = "IdxProductLocation", Order = 2, Unique = true)]
    public int LocationId { get; set; }

    public decimal Stock { get; set; } = 0m;

    public DateTime LastUpdated { get; set; } = DateTime.Now;

    [Ignore]
    public Product Product { get; set; }

    [Ignore]
    public ShopLocation Location { get; set; }
}
