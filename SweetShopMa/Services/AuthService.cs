using SweetShopMa.Models;
using SweetShopMa.Utils;

namespace SweetShopMa.Services;

/// <summary>
/// Handles user authentication and authorization.
/// 
/// WHAT IS AUTHSERVICE?
/// AuthService manages user login/logout and provides role/permission checks throughout the app.
/// It maintains the current logged-in user and exposes properties to check roles and permissions.
/// 
/// KEY RESPONSIBILITIES:
/// - User login (validate credentials)
/// - User logout (clear current user)
/// - Track current logged-in user
/// - Provide role checks (IsDeveloper, IsAdmin, etc.)
/// - Provide permission checks (CanManageUsers, CanRestock, etc.)
/// - Notify when user changes (for UI updates)
/// 
/// SECURITY:
/// Uses PasswordHelper.VerifyPassword() to securely verify passwords against hashed values.
/// Passwords are never stored as plain text - they're hashed using PBKDF2.
/// </summary>
public class AuthService
{
    // ============================================
    // DEPENDENCIES
    // ============================================
    
    /// <summary>
    /// Database service for querying user data.
    /// </summary>
    private readonly DatabaseService _databaseService;
    
    /// <summary>
    /// Currently logged-in user (null if no user is logged in).
    /// </summary>
    private User _currentUser;

    // ============================================
    // PUBLIC PROPERTIES
    // ============================================
    
    /// <summary>
    /// Gets the currently logged-in user (null if not authenticated).
    /// </summary>
    public User CurrentUser => _currentUser;
    
    /// <summary>
    /// Returns true if a user is currently logged in.
    /// </summary>
    public bool IsAuthenticated => _currentUser != null;
    
    // ============================================
    // ROLE CHECK PROPERTIES
    // ============================================
    
    /// <summary>
    /// Returns true if current user has Developer role.
    /// Uses null-conditional operator (??) to return false if no user is logged in.
    /// </summary>
    public bool IsDeveloper => _currentUser?.IsDeveloper ?? false;
    
    /// <summary>
    /// Returns true if current user has Admin role.
    /// </summary>
    public bool IsAdmin => _currentUser?.IsAdmin ?? false;
    
    /// <summary>
    /// Returns true if current user has Moderator role.
    /// </summary>
    public bool IsModerator => _currentUser?.IsModerator ?? false;
    
    /// <summary>
    /// Returns true if current user has User role (normal user).
    /// </summary>
    public bool IsUser => _currentUser?.IsUser ?? false;
    
    // ============================================
    // PERMISSION CHECK PROPERTIES
    // ============================================
    
    /// <summary>
    /// Returns true if current user can manage (create, edit, disable) other users.
    /// Only Developer and Admin can manage users.
    /// </summary>
    public bool CanManageUsers => _currentUser?.CanManageUsers ?? false;
    
    /// <summary>
    /// Returns true if current user can manage stock (add products, edit products).
    /// Developer, Admin, and Moderator can manage stock.
    /// </summary>
    public bool CanManageStock => _currentUser?.CanManageStock ?? false;
    
    /// <summary>
    /// Returns true if current user can use the attendance tracker.
    /// Developer, Admin, and Moderator can use attendance tracker.
    /// </summary>
    public bool CanUseAttendanceTracker => _currentUser?.CanUseAttendanceTracker ?? false;
    
    /// <summary>
    /// Returns true if current user can restock products (add inventory).
    /// Developer, Admin, and Moderator can restock.
    /// </summary>
    public bool CanRestock => _currentUser?.CanRestock ?? false;

    // ============================================
    // EVENTS
    // ============================================
    
    /// <summary>
    /// Event fired when user logs in or logs out.
    /// ViewModels can subscribe to this to update UI when authentication state changes.
    /// </summary>
    public event Action<User> OnUserChanged;

    // ============================================
    // CONSTRUCTOR
    // ============================================
    
    /// <summary>
    /// Constructor - receives DatabaseService via dependency injection.
    /// </summary>
    public AuthService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    // ============================================
    // AUTHENTICATION METHODS
    // ============================================
    
    /// <summary>
    /// Attempts to log in a user with the provided username and password.
    /// 
    /// HOW IT WORKS:
    /// 1. Look up user by username in database
    /// 2. Check if user exists and is enabled
    /// 3. Verify password using PasswordHelper (secure password hashing)
    /// 4. If valid, set as current user and fire OnUserChanged event
    /// 5. Return true if login successful, false otherwise
    /// 
    /// SECURITY:
    /// - Passwords are hashed using PBKDF2 (never stored as plain text)
    /// - Disabled users cannot log in (IsEnabled check)
    /// - Returns false for invalid credentials (doesn't reveal which part failed)
    /// </summary>
    /// <param name="username">Username to log in with</param>
    /// <param name="inputPassword">Password to verify</param>
    /// <returns>True if login successful, false otherwise</returns>
    public async Task<bool> LoginAsync(string username, string inputPassword)
    {
        // Look up user in database
        var storedUser = await _databaseService.GetUserByUsernameAsync(username);

        // Check if user exists, is enabled, and password is correct
        if (storedUser != null && storedUser.IsEnabled &&
            PasswordHelper.VerifyPassword(inputPassword, storedUser.Password))
        {
            // Login successful - set as current user
            _currentUser = storedUser;
            // Notify subscribers that user has changed
            OnUserChanged?.Invoke(_currentUser);
            return true;
        }

        // Login failed (invalid username, disabled account, or wrong password)
        return false;
    }

    /// <summary>
    /// Logs out the current user.
    /// 
    /// HOW IT WORKS:
    /// 1. Clear current user (set to null)
    /// 2. Fire OnUserChanged event with null (notifies UI that user logged out)
    /// </summary>
    public void Logout()
    {
        _currentUser = null;
        // Notify subscribers that user has logged out
        OnUserChanged?.Invoke(null);
    }
}

