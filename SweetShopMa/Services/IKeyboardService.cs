using System.Windows.Input;

namespace SweetShopMa.Services;

/// <summary>
/// Interface for a service that handles keyboard shortcuts.
/// On Windows, this service hooks into the global Window KeyDown event.
/// </summary>
public interface IKeyboardService
{
    /// <summary>
    /// Initializes the keyboard handler for the specified window.
    /// This is typically called from App.xaml.cs or Window creation lifecycle events.
    /// </summary>
    void Initialize(object window);
}
