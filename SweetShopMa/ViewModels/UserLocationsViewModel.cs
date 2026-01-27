using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Resources;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

public partial class UserLocationsViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly IShopSettingsService _settingsService;

    [ObservableProperty] private ObservableCollection<User> _users = new();
    [ObservableProperty] private ObservableCollection<ShopLocation> _locations = new();
    [ObservableProperty] private ObservableCollection<UserLocation> _assignments = new();

    [ObservableProperty] private User _selectedUser;
    [ObservableProperty] private ShopLocation _selectedLocation;
    [ObservableProperty] private bool _isPrimary;
    [ObservableProperty] private bool _isBusy;

    public UserLocationsViewModel(DatabaseService db, IShopSettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            var userList = await _db.GetUsersAsync();
            Users = new ObservableCollection<User>(userList);

            var locationList = await _settingsService.GetLocationsAsync();
            Locations = new ObservableCollection<ShopLocation>(locationList);

            await LoadAssignmentsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAssignmentsAsync()
    {
        if (SelectedUser != null)
        {
            var list = await _db.GetUserLocationsAsync(SelectedUser.Id);
            Assignments = new ObservableCollection<UserLocation>(list);
        }
        else
        {
            Assignments.Clear();
        }
    }

    partial void OnSelectedUserChanged(User value)
    {
        _ = LoadAssignmentsAsync();
    }

    [RelayCommand]
    private async Task AssignAsync()
    {
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
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RemoveAssignmentAsync(UserLocation assignment)
    {
        bool confirm = await Shell.Current.DisplayAlert(Strings.Confirm, Strings.RemoveAssignmentConfirm, Strings.Yes, Strings.No);
        if (confirm)
        {
            IsBusy = true;
            try
            {
                await _db.RemoveUserFromLocationAsync(assignment.UserId, assignment.LocationId);
                await LoadAssignmentsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
