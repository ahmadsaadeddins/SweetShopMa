using System.ComponentModel;
using SQLite;

namespace SweetShopMa.Models;

public class CartItem : INotifyPropertyChanged
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Emoji { get; set; }
    
    private decimal _price;
    public decimal Price 
    { 
        get => _price;
        set 
        { 
            if (_price != value)
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(ItemTotal));
            }
        }
    }
    
    private decimal _quantity;
    public decimal Quantity 
    { 
        get => _quantity;
        set 
        { 
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(ItemTotal));
            }
        }
    }
    
    public bool IsSoldByWeight { get; set; } = false;

    public decimal ItemTotal => Price * Quantity;
    
    public string UnitLabel => IsSoldByWeight ? "kg" : "pcs";

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
