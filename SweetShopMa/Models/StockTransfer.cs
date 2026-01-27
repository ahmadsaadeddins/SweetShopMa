using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Audit trail for stock movements between locations.
/// </summary>
public class StockTransfer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ProductId { get; set; }
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public decimal Quantity { get; set; }

    public string Reason { get; set; } = "";
    public int? UserId { get; set; } // The user who performed the transfer
    public DateTime TransferDate { get; set; } = DateTime.Now;

    [Ignore]
    public Product Product { get; set; }

    [Ignore]
    public ShopLocation FromLocation { get; set; }

    [Ignore]
    public ShopLocation ToLocation { get; set; }

    [Ignore]
    public User User { get; set; }

    [Ignore]
    public string ProductName { get; set; } = "Unknown";
    [Ignore]
    public string FromLocationName { get; set; } = "Main";
    [Ignore]
    public string ToLocationName { get; set; } = "Destination";
}
