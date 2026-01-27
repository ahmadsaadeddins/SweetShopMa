using SweetShopMa.Models;

namespace SweetShopMa.Services;

public class ShopSettingsService : IShopSettingsService
{
    private readonly DatabaseService _db;
    private ShopLocation _activeLocation;

    public event EventHandler SettingsChanged;
    public event EventHandler LocationChanged;

    public ShopSettingsService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<ShopSettings> GetSettingsAsync()
    {
        return await _db.GetShopSettingsAsync();
    }

    public async Task SaveSettingsAsync(ShopSettings settings)
    {
        await _db.SaveShopSettingsAsync(settings);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<List<ShopLocation>> GetLocationsAsync()
    {
        return await _db.GetShopLocationsAsync();
    }

    public async Task<ShopLocation> GetActiveLocationAsync()
    {
        if (_activeLocation == null)
        {
            _activeLocation = await _db.GetActiveLocationAsync();
        }
        return _activeLocation;
    }

    public async Task<int> CreateLocationAsync(ShopLocation location)
    {
        var id = await _db.CreateShopLocationAsync(location);
        return id;
    }

    public async Task UpdateLocationAsync(ShopLocation location)
    {
        await _db.UpdateShopLocationAsync(location);
        if (_activeLocation?.Id == location.Id)
        {
            _activeLocation = location;
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task DeleteLocationAsync(ShopLocation location)
    {
        await _db.DeleteShopLocationAsync(location);
        if (_activeLocation?.Id == location.Id)
        {
            _activeLocation = null;
            await GetActiveLocationAsync();
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task SetActiveLocationAsync(int locationId)
    {
        await _db.UpdateActiveLocationAsync(locationId);
        _activeLocation = await _db.GetActiveLocationAsync();
        LocationChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<ProductStock> GetCurrentStockAsync(int productId)
    {
        var location = await GetActiveLocationAsync();
        if (location == null) return null;
        return await _db.GetProductStockAsync(productId, location.Id);
    }

    public async Task UpdateCurrentStockAsync(int productId, decimal quantityChange)
    {
        var location = await GetActiveLocationAsync();
        if (location == null) return;
        await _db.UpdateProductStockAsync(productId, location.Id, quantityChange);
    }

    public async Task<List<ShopLocation>> GetLocationsForUserAsync(int userId)
    {
        var assignments = await _db.GetUserLocationsAsync(userId);
        var locationIds = assignments.Select(a => a.LocationId).ToList();
        var allLocations = await _db.GetShopLocationsAsync();
        return allLocations.Where(l => locationIds.Contains(l.Id)).ToList();
    }
}
