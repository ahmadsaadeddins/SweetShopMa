namespace SweetShopMa.Services;

/// <summary>
/// Interface for opening cash drawer across different platforms
/// </summary>
public interface ICashDrawerService
{
    /// <summary>
    /// Opens the cash drawer by sending commands to the printer
    /// </summary>
    Task<bool> OpenDrawerAsync();
}

