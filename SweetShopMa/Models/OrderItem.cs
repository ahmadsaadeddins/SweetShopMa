using SQLite;

namespace SweetShopMa.Models;

public class OrderItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Emoji { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; } // Changed to decimal to support weight (kilos)
    public bool IsSoldByWeight { get; set; } = false;
    public decimal ItemTotal => Price * Quantity;
    
    public string UnitLabel => IsSoldByWeight ? "kg" : "pcs";
}

