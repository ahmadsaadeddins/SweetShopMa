using SweetShopMa.Models;
using SweetShopMa.Utils;

namespace SweetShopMa.Services;

public class AuthService
{
    private readonly DatabaseService _databaseService;
    private User _currentUser;

    public User CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.IsAdmin ?? false;

    public event Action<User> OnUserChanged;

    public AuthService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<bool> LoginAsync(string username, string inputPassword)
    {
        var storedUser = await _databaseService.GetUserByUsernameAsync(username);

        if (storedUser != null && PasswordHelper.VerifyPassword(inputPassword, storedUser.Password)) // In production, use password hashing
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

