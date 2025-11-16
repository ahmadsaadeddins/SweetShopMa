using SQLite;

namespace SweetShopMa.Models;

public class Order
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }           // ← NEW: Who placed the order
    public string UserName { get; set; }      // ← NEW: Cashier/user name (for display)

    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public string Status { get; set; } // = "Completed"; // Completed, Cancelled, etc.

    [Ignore]
    public string FormattedDate => OrderDate.ToString("yyyy-MM-dd HH:mm:ss");
}

