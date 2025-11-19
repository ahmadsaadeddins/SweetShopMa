using System.ComponentModel;
using SQLite;

namespace SweetShopMa.Models;

public class Product : INotifyPropertyChanged
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Emoji { get; set; }
    public string Barcode { get; set; } = ""; // Barcode for scanning/searching
    public decimal Price { get; set; }
    public decimal Stock { get; set; } = 0; // Inventory quantity (in kilos for weight-based products)
    public bool IsSoldByWeight { get; set; } = false; // If true, product is sold by kilo/weight
    
    private decimal _quantity = 0;
    
    [Ignore] // Don't store Quantity in database - it's just UI state
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public Product() { }

    public Product(int id, string name, string emoji, decimal price)
    {
        Id = id;
        Name = name;
        Emoji = emoji;
        Price = price;
    }

    public decimal TotalPrice => Price * Quantity;
    
    public string UnitLabel => IsSoldByWeight ? "KGS" : "PCS";

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
