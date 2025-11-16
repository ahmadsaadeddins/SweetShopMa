using SweetShopMa.Models;

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

    public async Task<bool> LoginAsync(string username, string password)
    {
        var user = await _databaseService.GetUserByUsernameAsync(username);
        
        if (user != null && user.Password == password) // In production, use password hashing
        {
            _currentUser = user;
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

