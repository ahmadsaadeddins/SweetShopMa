using SQLite;

namespace SweetShopMa.Models;

public class RestockRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductEmoji { get; set; }
    public decimal QuantityAdded { get; set; }
    public decimal StockBefore { get; set; }
    public decimal StockAfter { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public DateTime RestockDate { get; set; } = DateTime.UtcNow;
}

