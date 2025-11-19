namespace SweetShopMa.Services;

/// <summary>
/// Default cash drawer service implementation (no-op for non-Windows platforms)
/// </summary>
public class DefaultCashDrawerService : ICashDrawerService
{
    public Task<bool> OpenDrawerAsync()
    {
        // Not supported on this platform
        return Task.FromResult(false);
    }
}

