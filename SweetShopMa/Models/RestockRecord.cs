using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents a record of when inventory was restocked.
/// 
/// WHAT IS A RESTOCK RECORD?
/// A RestockRecord is an audit trail entry that tracks when someone added inventory
/// to a product. It records who restocked, when, how much was added, and the stock
/// levels before and after the restock.
/// 
/// PURPOSE:
/// This provides accountability and traceability for inventory management.
/// Managers can see who restocked what products and when, which is important for:
/// - Inventory auditing
/// - Detecting discrepancies
/// - Tracking restocking patterns
/// - Employee accountability
/// 
/// EXAMPLE:
/// If someone adds 10 kilos to a product that had 50 kilos, a RestockRecord is created:
/// - ProductId: 123
/// - QuantityAdded: 10
/// - StockBefore: 50
/// - StockAfter: 60
/// - UserId: 5 (the person who restocked)
/// - RestockDate: 2024-01-15 14:30:00
/// </summary>
public class RestockRecord
{
    // ============================================
    // DATABASE PROPERTIES
    // ============================================
    
    /// <summary>
    /// Unique identifier for the restock record.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the product that was restocked.
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Product name at the time of restock (denormalized for easy display).
    /// </summary>
    public string ProductName { get; set; }
    
    /// <summary>
    /// Product emoji at the time of restock (for visual display in reports).
    /// </summary>
    public string ProductEmoji { get; set; }
    
    /// <summary>
    /// How much inventory was added (in kilos for weight-based, units for unit-based).
    /// </summary>
    public decimal QuantityAdded { get; set; }
    
    /// <summary>
    /// Stock level BEFORE the restock operation.
    /// This is captured before adding the quantity, so we have a complete audit trail.
    /// </summary>
    public decimal StockBefore { get; set; }
    
    /// <summary>
    /// Stock level AFTER the restock operation.
    /// This should equal StockBefore + QuantityAdded.
    /// </summary>
    public decimal StockAfter { get; set; }
    
    /// <summary>
    /// ID of the user (employee) who performed the restock.
    /// This tracks accountability - who added the inventory.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Name of the user who restocked (for display purposes).
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Date and time when the restock occurred (defaults to current UTC time).
    /// </summary>
    public DateTime RestockDate { get; set; } = DateTime.UtcNow;
}

