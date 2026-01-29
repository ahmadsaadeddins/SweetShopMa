using CommunityToolkit.Mvvm.ComponentModel;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides common functionality like IsBusy and Status messages.
/// </summary>
public partial class BaseViewModel : ObservableObject, IDisposable
{
    protected readonly DatabaseService _databaseService;
    protected readonly AuthService _authService;
    protected readonly Services.LocalizationService _localizationService;
    protected readonly Services.LoggingService _loggingService;

    private CancellationTokenSource _statusClearCts;
    private Task _statusClearTask;
    private bool _isDisposed = false;

    [ObservableProperty]
    private bool _isBusy;

    partial void OnIsBusyChanged(bool value)
    {
        _loggingService?.LogDebug(this.GetType().Name, $"IsBusy changed to: {value}");
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isErrorStatus;

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public BaseViewModel(
        DatabaseService databaseService, 
        AuthService authService, 
        Services.LocalizationService localizationService,
        Services.LoggingService loggingService = null)
    {
        _databaseService = databaseService;
        _authService = authService;
        _localizationService = localizationService;
        _loggingService = loggingService;
    }

    protected void ShowStatus(string message, bool isError = false)
    {
        _loggingService?.LogDebug(this.GetType().Name, $"Showing status: '{message}' (IsError: {isError})");
        
        // Cancel previous auto-clear task
        _statusClearCts?.Cancel();
        _statusClearCts?.Dispose();
        _statusClearCts = new CancellationTokenSource();
        
        // Note: Previous task is cancelled via CancellationToken and will exit gracefully.
        // We don't await it to avoid blocking the UI thread.
        // Task cleanup is handled in Dispose() method.
        
        // Set both properties atomically to avoid race condition
        // Set IsErrorStatus first, then StatusMessage to ensure the event handler sees the correct IsErrorStatus value
        IsErrorStatus = isError;
        StatusMessage = message;
        
        // Auto-clear status message after 5 seconds
        _statusClearTask = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(5000, _statusClearCts.Token);
                
                // Ensure we're on the main thread for UI updates
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (!_isDisposed && StatusMessage == message && !_statusClearCts.Token.IsCancellationRequested)
                    {
                        StatusMessage = "";
                    }
                });
            }
            catch (TaskCanceledException)
            {
                // Expected when a new status message arrives
            }
        }, _statusClearCts.Token);
    }

    /// <summary>
    /// Disposes of resources used by this ViewModel.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Cancel any pending status clear task
        _statusClearCts?.Cancel();
        _statusClearCts?.Dispose();
        
        // Clear task reference to allow garbage collection
        // Task will complete gracefully due to cancellation token
        _statusClearTask = null;
    }
}
