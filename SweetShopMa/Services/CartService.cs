using SweetShopMa.Models;

namespace SweetShopMa.Services;

public class CartService
{
    private readonly DatabaseService _databaseService;
    private List<CartItem> _cartItems = new();
    public event Action OnCartChanged;

    public CartService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task InitializeAsync()
    {
        _cartItems = await _databaseService.GetCartItemsAsync();
    }

    public List<CartItem> GetCartItems() => _cartItems;

    public decimal GetTotal() => _cartItems.Sum(x => x.ItemTotal);

    public async Task<bool> AddToCartAsync(Product product, decimal quantity)
    {
        if (quantity <= 0) return false;

        // Check stock availability
        var newQuantity = quantity;
        var existingItem = _cartItems.FirstOrDefault(x => x.ProductId == product.Id);
        if (existingItem is not null)
        {
            newQuantity += existingItem.Quantity;
        }

        var isAvailable = await _databaseService.CheckStockAvailabilityAsync(product.Id, newQuantity);
        if (!isAvailable)
        {
            return false; // Not enough stock
        }

        if (existingItem is not null)
        {
            // Update quantity - this will trigger property change notification
            existingItem.Quantity += quantity;
            await _databaseService.SaveCartItemAsync(existingItem);
            // Note: The existingItem object reference stays the same, so ObservableCollection will see the change
        }
        else
        {
            var cartItem = new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Emoji = product.Emoji,
                Price = product.Price,
                Quantity = quantity,
                IsSoldByWeight = product.IsSoldByWeight
            };
            await _databaseService.SaveCartItemAsync(cartItem);
            _cartItems.Add(cartItem);
        }

        OnCartChanged?.Invoke();
        return true;
    }

    public async Task RemoveFromCartAsync(CartItem item)
    {
        await _databaseService.DeleteCartItemAsync(item);
        _cartItems.Remove(item);
        OnCartChanged?.Invoke();
    }

    public async Task<Order> CheckoutAsync()
    {
        if (_cartItems.Count == 0) return null;

        // Create order
        var order = new Order
        {
            OrderDate = DateTime.Now,
            Total = GetTotal(),
            ItemCount = (int)Math.Ceiling(_cartItems.Sum(x => x.Quantity)), // Round up for display
            Status = "Completed"
        };

        var orderId = await _databaseService.CreateOrderAsync(order);
        order.Id = orderId;

        // Create order items and update inventory
        foreach (var cartItem in _cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Name = cartItem.Name,
                Emoji = cartItem.Emoji,
                Price = cartItem.Price,
                Quantity = cartItem.Quantity,
                IsSoldByWeight = cartItem.IsSoldByWeight
            };
            await _databaseService.CreateOrderItemAsync(orderItem);

            // Update inventory (reduce stock)
            await _databaseService.UpdateProductStockAsync(cartItem.ProductId, -cartItem.Quantity);
        }

        // Clear cart
        await _databaseService.ClearCartAsync();
        _cartItems.Clear();
        OnCartChanged?.Invoke();

        return order;
    }
}
