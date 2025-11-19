using System.ComponentModel;
using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents a product/item in the shop inventory.
/// 
/// WHAT IS A MODEL?
/// A Model is a data structure that represents a real-world entity (in this case, a product).
/// Models define what data is stored in the database and how it's structured.
/// 
/// INotifyPropertyChanged INTERFACE:
/// This interface allows the Product to notify the UI when its properties change.
/// When a property changes, the UI automatically updates (data binding).
/// 
/// EXAMPLE:
/// When Quantity changes, TotalPrice automatically recalculates and the UI updates.
/// </summary>
public class Product : INotifyPropertyChanged
{
    // ============================================
    // DATABASE PROPERTIES (Stored in SQLite)
    // ============================================
    
    /// <summary>
    /// Unique identifier for the product.
    /// [PrimaryKey, AutoIncrement] means SQLite will automatically generate unique IDs.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// Product name (e.g., "Chocolate Bar", "Candy Corn").
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Emoji/icon representing the product (e.g., "🍫", "🍬").
    /// Used for visual display in the UI.
    /// </summary>
    public string Emoji { get; set; }
    
    /// <summary>
    /// Barcode for scanning/searching products quickly.
    /// Can be empty string if product doesn't have a barcode.
    /// </summary>
    public string Barcode { get; set; } = "";
    
    /// <summary>
    /// Price per unit (for unit-based products) or per kilo (for weight-based products).
    /// Using decimal type for precise currency calculations (avoids floating-point errors).
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Current inventory quantity.
    /// - For unit-based products: Number of items (e.g., 100 pieces)
    /// - For weight-based products: Weight in kilos (e.g., 50.5 kilos)
    /// </summary>
    public decimal Stock { get; set; } = 0;
    
    /// <summary>
    /// Whether this product is sold by weight (true) or by unit (false).
    /// - true: Product is sold by kilo/weight (e.g., candy sold by weight)
    /// - false: Product is sold by piece/unit (e.g., individual chocolate bars)
    /// </summary>
    public bool IsSoldByWeight { get; set; } = false;
    
    // ============================================
    // UI-ONLY PROPERTIES (Not stored in database)
    // ============================================
    
    /// <summary>
    /// Quantity selected in the UI (for adding to cart).
    /// This is temporary UI state, not stored in the database.
    /// [Ignore] attribute tells SQLite to skip this property when saving/loading.
    /// </summary>
    private decimal _quantity = 0;
    
    [Ignore] // Don't store Quantity in database - it's just UI state
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            // Only update if value actually changed (prevents unnecessary UI updates)
            if (_quantity != value)
            {
                _quantity = value;
                // Notify UI that Quantity changed
                OnPropertyChanged(nameof(Quantity));
                // Also notify that TotalPrice changed (since it depends on Quantity)
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    // ============================================
    // CONSTRUCTORS
    // ============================================
    
    /// <summary>
    /// Default constructor (required for SQLite).
    /// </summary>
    public Product() { }

    /// <summary>
    /// Constructor for creating a product with initial values.
    /// </summary>
    public Product(int id, string name, string emoji, decimal price)
    {
        Id = id;
        Name = name;
        Emoji = emoji;
        Price = price;
    }

    // ============================================
    // COMPUTED PROPERTIES (Calculated values)
    // ============================================
    
    /// <summary>
    /// Calculated total price: Price × Quantity.
    /// This is a computed property (no setter) that automatically calculates the total.
    /// Example: If Price = 5.50 and Quantity = 2, then TotalPrice = 11.00
    /// </summary>
    public decimal TotalPrice => Price * Quantity;
    
    /// <summary>
    /// Unit label for display purposes.
    /// Returns "KGS" (kilograms) for weight-based products, "PCS" (pieces) for unit-based.
    /// Used in UI to show the correct unit next to quantities.
    /// </summary>
    public string UnitLabel => IsSoldByWeight ? "KGS" : "PCS";

    // ============================================
    // INotifyPropertyChanged IMPLEMENTATION
    // ============================================
    
    /// <summary>
    /// Event that fires when a property value changes.
    /// The UI subscribes to this event to automatically update when data changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event to notify the UI that a property has changed.
    /// [CallerMemberName] automatically fills in the property name if not provided.
    /// 
    /// EXAMPLE:
    /// OnPropertyChanged(); // Automatically uses the calling property's name
    /// OnPropertyChanged(nameof(TotalPrice)); // Explicitly specify property name
    /// </summary>
    protected void OnPropertyChanged(string propertyName = "")
    {
        // ?. is null-conditional operator - only invokes if PropertyChanged is not null
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
