using SweetShopMa.Models;

namespace SweetShopMa.Services;

/// <summary>
/// Manages shopping cart operations including adding, removing, and checking out items.
/// Triggers OnCartChanged event when cart state changes to notify UI.
/// </summary>
public class CartService
{
    private readonly DatabaseService _databaseService;
    private readonly SessionContext _sessionContext;
    private List<CartItem> _cartItems = new();
    
    public event Action OnCartChanged;

    public CartService(DatabaseService databaseService, SessionContext sessionContext)
    {
        _databaseService = databaseService;
        _sessionContext = sessionContext;
    }

    /// <summary>
    /// Initializes the cart by loading saved items from the database.
    /// Must be called before using the cart.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _cartItems = await _databaseService.GetCartItemsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing cart: {ex}");
            _cartItems = new List<CartItem>(); // Fallback to empty cart
        }
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
    /// <returns>ServiceResult indicating success or failure with message</returns>
    public async Task<ServiceResult> AddToCartAsync(Product product, decimal quantity)
    {
        try
        {
            if (product == null) 
                return ServiceResult.Fail("Product cannot be null");
            
            if (quantity <= 0)
                return ServiceResult.Fail("Quantity must be greater than zero");

            if (quantity > 1000) // Sanity check for weight-based items
                return ServiceResult.Fail("Quantity exceeds maximum limit");

            // Calculate new quantity if product already in cart
            var newQuantity = quantity;
            var existingItem = _cartItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (existingItem is not null)
            {
                newQuantity += existingItem.Quantity;
            }

            // Check stock availability
            var isAvailable = await _databaseService.CheckStockAvailabilityAsync(product.Id, newQuantity, _sessionContext.ActiveLocation.Id);
            if (!isAvailable)
            {
                return ServiceResult.Fail("Insufficient stock available");
            }

            if (existingItem is not null)
            {
                existingItem.Quantity += quantity;
                await _databaseService.SaveCartItemAsync(existingItem);
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
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding to cart: {ex}");
            return ServiceResult.Fail($"Internal error: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public async Task RemoveFromCartAsync(CartItem item)
    {
        try
        {
            if (item == null) return;
            
            await _databaseService.DeleteCartItemAsync(item);
            _cartItems.Remove(item);
            OnCartChanged?.Invoke();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing from cart: {ex}");
            // Still try to remove from memory even if database operation fails
            _cartItems.Remove(item);
            OnCartChanged?.Invoke();
        }
    }

    /// <summary>
    /// Updates the quantity of an item already in the cart.
    /// Validates stock availability before updating.
    /// </summary>
    /// <returns>ServiceResult indicating success or failure with message</returns>
    public async Task<ServiceResult> UpdateCartItemQuantityAsync(CartItem item, decimal newQuantity)
    {
        try
        {
            if (item == null) return ServiceResult.Fail("Item cannot be null");
            
            if (newQuantity <= 0)
            {
                await RemoveFromCartAsync(item);
                return ServiceResult.Ok("Item removed from cart");
            }

            if (newQuantity > 1000)
                return ServiceResult.Fail("Quantity exceeds maximum limit");

            // Check stock availability
            var isAvailable = await _databaseService.CheckStockAvailabilityAsync(item.ProductId, newQuantity, _sessionContext.ActiveLocation.Id);
            if (!isAvailable)
            {
                return ServiceResult.Fail("Insufficient stock available");
            }

            item.Quantity = newQuantity;
            await _databaseService.SaveCartItemAsync(item);
            OnCartChanged?.Invoke();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating cart item quantity: {ex}");
            return ServiceResult.Fail($"Internal error: {ex.Message}");
        }
    }

    /// <summary>
    /// Completes the checkout process: creates an order, updates inventory, and clears cart.
    /// </summary>
    /// <returns>Created Order object if successful, null if cart is empty or error occurred</returns>
    public async Task<Order> CheckoutAsync(int userId, string userName)
    {
        if (_cartItems.Count == 0) 
            return null;

        // Create order with current cart totals
        var order = new Order
        {
            UserId = userId,
            UserName = userName,
            OrderDate = DateTime.Now,
            Total = GetTotal(),
            ItemCount = (int)Math.Ceiling(_cartItems.Sum(x => x.Quantity)), // Round up for display
            Status = "Completed"
        };

        // Save order and get its ID
        // Use transaction to ensure data integrity
        // Exceptions from DatabaseService will propagate to the ViewModel
        var resultOrder = await _databaseService.ProcessCheckoutAsync(order, _cartItems, _sessionContext.ActiveLocation.Id);
        
        if (resultOrder == null)
        {
            System.Diagnostics.Debug.WriteLine("Checkout failed: ProcessCheckoutAsync returned null");
            return null;
        }

        // Clear cart memory only after successful checkout
        _cartItems.Clear();
        OnCartChanged?.Invoke();

        return resultOrder;
    }
}
