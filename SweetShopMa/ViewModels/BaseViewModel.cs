using CommunityToolkit.Mvvm.ComponentModel;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides common functionality like IsBusy and Status messages.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    protected readonly DatabaseService _databaseService;
    protected readonly AuthService _authService;
    protected readonly Services.LocalizationService _localizationService;
    protected readonly Services.LoggingService _loggingService;

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
        StatusMessage = message;
        IsErrorStatus = isError;
        
        // Auto-clear status message after 5 seconds
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            if (StatusMessage == message)
            {
                StatusMessage = "";
            }
        });
    }
}
