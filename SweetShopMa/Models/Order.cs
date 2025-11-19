using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents a completed order/transaction.
/// 
/// WHAT IS AN ORDER?
/// An Order is a record of a completed purchase. When a customer checks out,
/// an Order is created with all the details of what was purchased, when,
/// and by whom (which cashier).
/// 
/// RELATIONSHIP TO ORDERITEMS:
/// One Order can have many OrderItems (one-to-many relationship).
/// For example, an order might contain:
/// - 2 chocolate bars (OrderItem 1)
/// - 0.5 kilos of candy (OrderItem 2)
/// - 3 lollipops (OrderItem 3)
/// 
/// All three OrderItems belong to the same Order.
/// </summary>
public class Order
{
    // ============================================
    // DATABASE PROPERTIES
    // ============================================
    
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the user (cashier) who placed the order.
    /// This tracks which employee processed the sale.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Name of the cashier/user who placed the order (for display purposes).
    /// Stored as a string for easy display without needing to join with User table.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Date and time when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }
    
    /// <summary>
    /// Total amount of the order (sum of all OrderItems).
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Total number of items in the order (for display purposes).
    /// This is the sum of quantities from all OrderItems.
    /// </summary>
    public int ItemCount { get; set; }
    
    /// <summary>
    /// Order status: "Completed", "Cancelled", etc.
    /// Currently, all orders are "Completed" when created.
    /// </summary>
    public string Status { get; set; }

    // ============================================
    // COMPUTED PROPERTIES (Not stored in database)
    // ============================================
    
    /// <summary>
    /// Formatted date string for display in the UI.
    /// Format: "yyyy-MM-dd HH:mm:ss" (e.g., "2024-01-15 14:30:00")
    /// [Ignore] attribute tells SQLite to skip this property.
    /// </summary>
    [Ignore]
    public string FormattedDate => OrderDate.ToString("yyyy-MM-dd HH:mm:ss");
}

