using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Resources;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

public partial class LocationsViewModel : ObservableObject
{
    private readonly IShopSettingsService _settingsService;
    private readonly SessionContext _sessionContext;

    [ObservableProperty] private ObservableCollection<ShopLocation> _locations = new();
    [ObservableProperty] private ShopLocation _selectedLocation;
    [ObservableProperty] private bool _isBusy;

    // Form fields for adding/editing
    [ObservableProperty] private string _newLocationName = "";
    [ObservableProperty] private string _newAddress = "";
    [ObservableProperty] private string _newPhoneNumber = "";
    [ObservableProperty] private bool _isPrimary;
    [ObservableProperty] private bool _isEditMode;

    public LocationsViewModel(IShopSettingsService settingsService, SessionContext sessionContext)
    {
        _settingsService = settingsService;
        _sessionContext = sessionContext;
        LoadLocationsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadLocationsAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _settingsService.GetLocationsAsync();
            Locations = new ObservableCollection<ShopLocation>(list);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveLocationAsync()
    {
        if (string.IsNullOrWhiteSpace(NewLocationName))
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.LocationNameIsRequired, Strings.OK);
            return;
        }

        IsBusy = true;
        try
        {
            if (IsEditMode && SelectedLocation != null)
            {
                SelectedLocation.LocationName = NewLocationName;
                SelectedLocation.Address = NewAddress;
                SelectedLocation.PhoneNumber = NewPhoneNumber;
                SelectedLocation.IsPrimary = IsPrimary;
                await _settingsService.UpdateLocationAsync(SelectedLocation);
            }
            else
            {
                var location = new ShopLocation
                {
                    LocationName = NewLocationName,
                    Address = NewAddress,
                    PhoneNumber = NewPhoneNumber,
                    IsPrimary = IsPrimary,
                    IsActive = true
                };
                await _settingsService.CreateLocationAsync(location);
            }

            ClearForm();
            await LoadLocationsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void EditLocation(ShopLocation location)
    {
        SelectedLocation = location;
        NewLocationName = location.LocationName;
        NewAddress = location.Address;
        NewPhoneNumber = location.PhoneNumber;
        IsPrimary = location.IsPrimary;
        IsEditMode = true;
    }

    [RelayCommand]
    private async Task DeleteLocationAsync(ShopLocation location)
    {
        if (location.IsPrimary)
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.CannotDeletePrimaryLocation, Strings.OK);
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(Strings.Confirm, string.Format(Strings.DeleteLocationConfirm, location.LocationName), Strings.Yes, Strings.No);
        if (confirm)
        {
            await _settingsService.DeleteLocationAsync(location);
            await LoadLocationsAsync();
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        NewLocationName = "";
        NewAddress = "";
        NewPhoneNumber = "";
        IsPrimary = false;
        IsEditMode = false;
        SelectedLocation = null;
    }
}
