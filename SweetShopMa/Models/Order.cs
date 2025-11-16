using SQLite;

namespace SweetShopMa.Models;

public class Order
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public string Status { get; set; } = "Completed"; // Completed, Cancelled, etc.
}

