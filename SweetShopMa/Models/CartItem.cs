using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents an item in the shopping cart.
/// Implements INotifyPropertyChanged to support real-time UI updates
/// when quantity or price changes.
/// </summary>
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
        set => SetProperty(ref _price, value, () =>
        {
            OnPropertyChanged(nameof(ItemTotal));
        });
    }

    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value, () =>
        {
            OnPropertyChanged(nameof(ItemTotal));
        });
    }

    public bool IsSoldByWeight { get; set; } = false;

    /// <summary>
    /// Computed property: Price × Quantity
    /// </summary>
    public decimal ItemTotal => Price * Quantity;

    /// <summary>
    /// Unit label for display ("KGS" for weight-based, "PCS" for unit-based)
    /// </summary>
    public string UnitLabel => IsSoldByWeight ? "KGS" : "PCS";

    public CartItem() { }

    public CartItem(int productId, string name, string emoji, decimal price, decimal quantity, bool isSoldByWeight = false)
    {
        ProductId = productId;
        Name = name;
        Emoji = emoji;
        Price = price;
        Quantity = quantity;
        IsSoldByWeight = isSoldByWeight;
    }

    /// <summary>
    /// Generic property setter that handles change detection and triggers property changed events.
    /// </summary>
    private void SetProperty<T>(ref T field, T value, Action onChanged = null, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises PropertyChanged event for the specified property name.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
