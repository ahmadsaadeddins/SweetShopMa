using SweetShopMa.Models;

namespace SweetShopMa.Services;

/// <summary>
/// Manages shopping cart operations including adding, removing, and checking out items.
/// Triggers OnCartChanged event when cart state changes to notify UI.
/// </summary>
public class CartService
{
    private readonly DatabaseService _databaseService;
    private List<CartItem> _cartItems = new();
    
    /// <summary>
    /// Fired when cart contents or totals change (item added, removed, or quantity modified)
    /// </summary>
    public event Action OnCartChanged;

    public CartService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Initializes the cart by loading saved items from the database.
    /// Must be called before using the cart.
    /// </summary>
    public async Task InitializeAsync()
    {
        _cartItems = await _databaseService.GetCartItemsAsync();
    }

    /// <summary>
    /// Returns a copy of the current cart items.
    /// </summary>
    public List<CartItem> GetCartItems() => _cartItems;

    /// <summary>
    /// Calculates total cart value (sum of all ItemTotals)
    /// </summary>
    public decimal GetTotal() => _cartItems.Sum(x => x.ItemTotal);

    /// <summary>
    /// Adds a product to the cart or increments quantity if already present.
    /// Validates stock availability before adding.
    /// </summary>
    /// <returns>True if successful, false if insufficient stock</returns>
    public async Task<bool> AddToCartAsync(Product product, decimal quantity)
    {
        if (quantity <= 0) 
            return false;

        // Calculate new quantity if product already in cart
        var newQuantity = quantity;
        var existingItem = _cartItems.FirstOrDefault(x => x.ProductId == product.Id);
        if (existingItem is not null)
        {
            newQuantity += existingItem.Quantity;
        }

        // Check stock availability
        var isAvailable = await _databaseService.CheckStockAvailabilityAsync(product.Id, newQuantity);
        if (!isAvailable)
        {
            return false; // Not enough stock
        }

        if (existingItem is not null)
        {
            // Update existing item - CartItem's INotifyPropertyChanged will notify UI
            existingItem.Quantity += quantity;
            await _databaseService.SaveCartItemAsync(existingItem);
            // UI will automatically update because CartItem.Quantity triggers PropertyChanged
        }
        else
        {
            // Create new cart item
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

        // Notify UI that cart has changed
        OnCartChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public async Task RemoveFromCartAsync(CartItem item)
    {
        await _databaseService.DeleteCartItemAsync(item);
        _cartItems.Remove(item);
        OnCartChanged?.Invoke();
    }

    /// <summary>
    /// Updates the quantity of an item already in the cart.
    /// Validates stock availability before updating.
    /// </summary>
    /// <returns>True if successful, false if insufficient stock</returns>
    public async Task<bool> UpdateCartItemQuantityAsync(CartItem item, decimal newQuantity)
    {
        if (newQuantity <= 0)
        {
            await RemoveFromCartAsync(item);
            return true;
        }

        // Check stock availability
        var isAvailable = await _databaseService.CheckStockAvailabilityAsync(item.ProductId, newQuantity);
        if (!isAvailable)
        {
            return false; // Not enough stock
        }

        item.Quantity = newQuantity;
        await _databaseService.SaveCartItemAsync(item);
        OnCartChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Completes the checkout process: creates an order, updates inventory, and clears cart.
    /// </summary>
    /// <returns>Created Order object if successful, null if cart is empty</returns>
    public async Task<Order> CheckoutAsync(int userId, string userName)
    {
        if (_cartItems.Count == 0) 
            return null;

        // Create order with current cart totals
        var order = new Order
        {
            UserId = userId,           // ← NEW
            UserName = userName,       // ← NEW
            OrderDate = DateTime.Now,
            Total = GetTotal(),
            ItemCount = (int)Math.Ceiling(_cartItems.Sum(x => x.Quantity)), // Round up for display
            Status = "Completed"
        };

        // Save order and get its ID
        var orderId = await _databaseService.CreateOrderAsync(order);
        order.Id = orderId;

        // Create order items and update inventory
        foreach (var cartItem in _cartItems)
        {
            // Create order item record
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

            // Reduce product stock by quantity sold
            await _databaseService.UpdateProductStockAsync(cartItem.ProductId, -cartItem.Quantity);
        }

        // Clear cart
        await _databaseService.ClearCartAsync();
        _cartItems.Clear();
        OnCartChanged?.Invoke();

        return order;
    }
}
