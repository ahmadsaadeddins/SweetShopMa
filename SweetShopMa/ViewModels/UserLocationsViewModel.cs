using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Resources;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

public partial class UserLocationsViewModel : ObservableObject, IDisposable
{
    private readonly DatabaseService _db;
    private readonly IShopSettingsService _settingsService;
    private readonly AuthService _authService;
    private readonly LocalizationService _localizationService;
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _localizationService?.LanguageChanged -= OnLanguageChanged;
        _disposed = true;
        GC.SuppressFinalize(this);
    }



    [ObservableProperty] private ObservableCollection<User> _users = new();
    [ObservableProperty] private ObservableCollection<ShopLocation> _locations = new();
    [ObservableProperty] private ObservableCollection<UserLocation> _assignments = new();

    [ObservableProperty] private User? _selectedUser;
    [ObservableProperty] private ShopLocation? _selectedLocation;
    [ObservableProperty] private bool _isPrimary;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private string _assignedLocationText = "";
    [ObservableProperty] private string _primaryText = "";
    [ObservableProperty] private string _removeText = "";



    public UserLocationsViewModel(DatabaseService db, IShopSettingsService settingsService, AuthService authService, LocalizationService localizationService)
    {
        _db = db;
        _settingsService = settingsService;
        _authService = authService;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedStrings();
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        AssignedLocationText = _localizationService.GetString("AssignedLocation");
        PrimaryText = _localizationService.GetString("Primary");
        RemoveText = _localizationService.GetString("Remove");
    }

    public async Task InitializeAsync()

    {
        await LoadDataAsync();
    }


    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var userList = await _db.GetUsersAsync();
            UpdateCollection(Users, userList);

            var locationList = await _settingsService.GetLocationsAsync();
            UpdateCollection(Locations, locationList);

            await LoadAssignmentsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            await Shell.Current.DisplayAlert(Strings.Error, Strings.FailedToLoadAssignments, Strings.OK);
        }
        finally
        {
            IsBusy = false;
        }
    }


    private async Task LoadAssignmentsAsync()
    {
        try
        {
            if (SelectedUser != null)
            {
                var list = await _db.GetUserLocationsAsync(SelectedUser.Id);
                UpdateCollection(Assignments, list);
            }
            else
            {
                Assignments.Clear();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading assignments: {ex.Message}");
            await Shell.Current.DisplayAlert(Strings.Error, Strings.FailedToLoadAssignments, Strings.OK);
        }
    }

    private void UpdateCollection<T>(ObservableCollection<T> collection, IEnumerable<T> newItems)
    {
        collection.Clear();
        foreach (var item in newItems)
            collection.Add(item);
    }


    partial void OnSelectedUserChanged(User? value)
    {
        _ = LoadAssignmentsAsync();
    }


    [RelayCommand]
    private async Task AssignAsync()
    {
        if (!_authService.CanManageUsers)
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.UnauthorizedAction, Strings.OK);
            return;
        }

        if (SelectedUser == null || SelectedLocation == null)
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.PleaseSelectUserAndLocation, Strings.OK);
            return;
        }

        IsBusy = true;
        try
        {
            await _db.AssignUserToLocationAsync(SelectedUser.Id, SelectedLocation.Id, IsPrimary);
            await LoadAssignmentsAsync();
            await Shell.Current.DisplayAlert(Strings.Success, Strings.AssignmentSuccess, Strings.OK);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.OK);
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private async Task RemoveAssignmentAsync(UserLocation assignment)
    {
        if (!_authService.CanManageUsers)
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.UnauthorizedAction, Strings.OK);
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(Strings.Confirm, Strings.RemoveAssignmentConfirm, Strings.Yes, Strings.No);
        if (confirm)
        {
            IsBusy = true;
            try
            {
                await _db.RemoveUserFromLocationAsync(assignment.UserId, assignment.LocationId);
                await LoadAssignmentsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.OK);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

}
