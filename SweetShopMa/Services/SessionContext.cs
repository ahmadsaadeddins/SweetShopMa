using SweetShopMa.Models;

namespace SweetShopMa.Services;

/// <summary>
/// Holds session-specific data like the current logged-in user and active location.
/// This service is intended to be registered as a Scoped or Singleton service 
/// depending on whether we want to support multiple windows/sessions in the future.
/// Currently registered as Scoped in MauiProgram.
/// </summary>
public class SessionContext
{
    private readonly AuthService _authService;
    private readonly IShopSettingsService _settingsService;

    public SessionContext(AuthService authService, IShopSettingsService settingsService)
    {
        _authService = authService;
        _settingsService = settingsService;
    }

    public User CurrentUser => _authService.CurrentUser;
    public ShopLocation ActiveLocation { get; set; }

    public async Task InitializeAsync()
    {
        ActiveLocation = await _settingsService.GetActiveLocationAsync();
    }

    public void SetActiveLocation(ShopLocation location)
    {
        ActiveLocation = location;
    }
}
