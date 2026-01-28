using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;
using SweetShopMa.Utils;

namespace SweetShopMa.ViewModels;

/// <summary>
/// ViewModel for managing users (employees).
/// Extracted from AdminViewModel to follow SRP.
/// </summary>
public partial class UserManagementViewModel : BaseViewModel
{
    private const int MinPasswordLength = 4;
    private const decimal MaxSalaryAmount = 10000000m;

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private string _newUserName = "";

    [ObservableProperty]
    private string _newUserUsername = "";

    [ObservableProperty]
    private string _newUserPassword = "";

    [ObservableProperty]
    private bool _newUserIsAdmin = true;

    [ObservableProperty]
    private string _newUserSalary = "0";

    public bool CanManageUsers => _authService.CanManageUsers;

    public UserManagementViewModel(
        DatabaseService databaseService, 
        AuthService authService, 
        Services.LocalizationService localizationService,
        Services.LoggingService loggingService) 
        : base(databaseService, authService, localizationService, loggingService)
    {
    }

    [RelayCommand(CanExecute = nameof(CanManageUsers))]
    public async Task LoadUsersAsync()
    {
        IsBusy = true;
        try
        {
            var users = await _databaseService.GetUsersAsync();
            Users.Clear();
            
            if (users != null)
            {
                foreach (var user in users.Where(u => u != null && !u.IsDeveloper))
                {
                    Users.Add(user);
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadUsersAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorLoadingUsers"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanManageUsers))]
    public async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUserName) ||
            string.IsNullOrWhiteSpace(NewUserUsername) ||
            string.IsNullOrWhiteSpace(NewUserPassword))
        {
            ShowStatus(_localizationService.GetString("PleaseFillAllUserFields"), true);
            return;
        }

        if (NewUserPassword.Length < MinPasswordLength)
        {
            ShowStatus(_localizationService.GetString("PasswordMinLength"), true);
            return;
        }

        if (!decimal.TryParse(NewUserSalary, NumberStyles.Number, CultureInfo.InvariantCulture, out var salary) || salary < 0 || salary > MaxSalaryAmount)
        {
            ShowStatus(_localizationService.GetString("EnterValidSalary"), true);
            return;
        }

        IsBusy = true;
        try
        {
            if (await _databaseService.UsernameExistsAsync(NewUserUsername.Trim()))
            {
                ShowStatus(_localizationService.GetString("UsernameExists"), true);
                return;
            }

            string selectedRole = NewUserIsAdmin ? RoleConstants.Admin : RoleConstants.User;
            
            var user = new User
            {
                Name = NewUserName.Trim(),
                Username = NewUserUsername.Trim(),
                Password = PasswordHelper.HashPassword(NewUserPassword.Trim()),
                Role = selectedRole,
                MonthlySalary = salary,
                OvertimeMultiplier = 1.5m
            };

            await _databaseService.CreateUserAsync(user);
            
            string userType = _localizationService.GetString(NewUserIsAdmin ? "Admin" : "User");
            string createdMsg = _localizationService.GetString("CreatedUser");
            ShowStatus(string.Format(createdMsg, userType, user.Name), false);

            await LoadUsersAsync();

            // Clear form
            NewUserName = "";
            NewUserUsername = "";
            NewUserPassword = "";
            NewUserIsAdmin = false;
            NewUserSalary = "0";
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("AddUserAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorCreatingUser"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanManageUsers))]
    public async Task ToggleUserStatusAsync(User user)
    {
        if (user == null) return;

        if (_authService.CurrentUser?.Id == user.Id && user.IsEnabled)
        {
            ShowStatus(_localizationService.GetString("CannotDisableCurrentAccount"), true);
            return;
        }

        IsBusy = true;
        try
        {
            user.IsEnabled = !user.IsEnabled;
            await _databaseService.UpdateUserAsync(user);
            
            string action = user.IsEnabled ? _localizationService.GetString("Enabled") : _localizationService.GetString("Disabled2");
            string statusMsg = _localizationService.GetString("UserStatusChanged");
            ShowStatus(string.Format(statusMsg, action, user.Name), false);
            
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ToggleUserStatusAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorUpdatingUser"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
