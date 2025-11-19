using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents a single item within an order.
/// 
/// WHAT IS AN ORDERITEM?
/// An OrderItem is one line item in an order. For example, if a customer buys
/// 2 chocolate bars and 0.5 kilos of candy, there will be 2 OrderItems in the Order.
/// 
/// RELATIONSHIP TO ORDER:
/// Many OrderItems belong to one Order (many-to-one relationship).
/// The OrderId property links the OrderItem to its parent Order.
/// 
/// WHY STORE PRODUCT INFO HERE?
/// OrderItem stores a copy of product information (Name, Emoji, Price) at the time
/// of purchase. This is important because:
/// 1. Product prices might change later, but we want to keep the original price
/// 2. Products might be deleted, but we still want order history
/// 3. This is called "denormalization" - storing redundant data for historical accuracy
/// </summary>
public class OrderItem
{
    // ============================================
    // DATABASE PROPERTIES
    // ============================================
    
    /// <summary>
    /// Unique identifier for the order item.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the parent Order that this item belongs to.
    /// This creates the relationship between Order and OrderItem.
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// ID of the Product that was purchased.
    /// This links back to the Product table (though product info is also stored here).
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Product name at the time of purchase (copied from Product).
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Product emoji at the time of purchase (copied from Product).
    /// </summary>
    public string Emoji { get; set; }
    
    /// <summary>
    /// Price per unit/kilo at the time of purchase (copied from Product).
    /// This preserves the original price even if the product's price changes later.
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Quantity purchased (can be decimal to support weight-based products).
    /// - For unit-based products: Number of items (e.g., 2.0)
    /// - For weight-based products: Weight in kilos (e.g., 0.5)
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Whether this product was sold by weight (true) or by unit (false).
    /// Copied from Product at time of purchase.
    /// </summary>
    public bool IsSoldByWeight { get; set; } = false;
    
    // ============================================
    // COMPUTED PROPERTIES (Not stored in database)
    // ============================================
    
    /// <summary>
    /// Calculated total for this line item: Price × Quantity.
    /// Example: If Price = 5.50 and Quantity = 2, then ItemTotal = 11.00
    /// </summary>
    public decimal ItemTotal => Price * Quantity;
    
    /// <summary>
    /// Unit label for display: "KGS" for weight-based, "PCS" for unit-based.
    /// </summary>
    public string UnitLabel => IsSoldByWeight ? "KGS" : "PCS";
}

