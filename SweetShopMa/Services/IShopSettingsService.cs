using SweetShopMa.Models;

namespace SweetShopMa.Services;

public interface IShopSettingsService
{
    Task<ShopSettings> GetSettingsAsync();
    Task SaveSettingsAsync(ShopSettings settings);
    Task<List<ShopLocation>> GetLocationsAsync();
    Task<ShopLocation> GetActiveLocationAsync();
    Task<int> CreateLocationAsync(ShopLocation location);
    Task UpdateLocationAsync(ShopLocation location);
    Task DeleteLocationAsync(ShopLocation location);
    Task SetActiveLocationAsync(int locationId);
    Task<List<ShopLocation>> GetLocationsForUserAsync(int userId);
    
    // Inventory helper using active location
    Task<ProductStock> GetCurrentStockAsync(int productId);
    Task UpdateCurrentStockAsync(int productId, decimal quantityChange);
    
    // Events for UI update notification
    event EventHandler SettingsChanged;
    event EventHandler LocationChanged;
}
