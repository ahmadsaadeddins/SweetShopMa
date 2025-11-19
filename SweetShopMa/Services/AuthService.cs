using SweetShopMa.Models;
using SweetShopMa.Utils;

namespace SweetShopMa.Services;

public class AuthService
{
    private readonly DatabaseService _databaseService;
    private User _currentUser;

    public User CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    
    // Role checks
    public bool IsDeveloper => _currentUser?.IsDeveloper ?? false;
    public bool IsAdmin => _currentUser?.IsAdmin ?? false;
    public bool IsModerator => _currentUser?.IsModerator ?? false;
    public bool IsUser => _currentUser?.IsUser ?? false;
    
    // Permission checks
    public bool CanManageUsers => _currentUser?.CanManageUsers ?? false;
    public bool CanManageStock => _currentUser?.CanManageStock ?? false;
    public bool CanUseAttendanceTracker => _currentUser?.CanUseAttendanceTracker ?? false;
    public bool CanRestock => _currentUser?.CanRestock ?? false;

    public event Action<User> OnUserChanged;

    public AuthService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<bool> LoginAsync(string username, string inputPassword)
    {
        var storedUser = await _databaseService.GetUserByUsernameAsync(username);

        if (storedUser != null && storedUser.IsEnabled &&
            PasswordHelper.VerifyPassword(inputPassword, storedUser.Password)) // In production, use password hashing
        {
            _currentUser = storedUser;
            OnUserChanged?.Invoke(_currentUser);
            return true;
        }

        return false;
    }

    public void Logout()
    {
        _currentUser = null;
        OnUserChanged?.Invoke(null);
    }
}

