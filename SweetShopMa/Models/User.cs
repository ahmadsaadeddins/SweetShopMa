using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents a user/employee in the system.
/// 
/// ROLE-BASED ACCESS CONTROL (RBAC):
/// This application uses a role-based permission system where different user roles
/// have different capabilities. The roles are hierarchical:
/// 
/// 1. Developer: Full access, can create users
/// 2. Admin: Can manage users, products, attendance, restock
/// 3. Moderator: Can manage stock, attendance, restock (but NOT users)
/// 4. User: Can only sell (use shop interface)
/// 
/// PERMISSIONS:
/// Permissions are computed properties that check the user's role and return true/false.
/// These are used throughout the app to show/hide features and enable/disable actions.
/// </summary>
public class User
{
    // ============================================
    // DATABASE PROPERTIES (Stored in SQLite)
    // ============================================
    
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// Login username (must be unique).
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Hashed password (stored securely using PBKDF2).
    /// In production, passwords should always be hashed, never stored as plain text.
    /// This app uses PasswordHelper.HashPassword() to hash passwords before storing.
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// User role: "Developer", "Admin", "Moderator", or "User".
    /// Default is "User" (lowest privilege level).
    /// </summary>
    public string Role { get; set; } = "User";
    
    /// <summary>
    /// User's full name (for display purposes).
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Date when the user account was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Whether the user account is enabled (active) or disabled.
    /// Disabled users cannot log in.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Employee's monthly salary (for payroll/attendance calculations).
    /// </summary>
    public decimal MonthlySalary { get; set; } = 0m;
    
    /// <summary>
    /// Overtime multiplier for calculating overtime pay (1.0, 1.5, or 2.0).
    /// Default is 1.5 (time and a half).
    /// </summary>
    public decimal OvertimeMultiplier { get; set; } = 1.5m;
    
    // ============================================
    // ROLE CHECK PROPERTIES (Computed)
    // ============================================
    
    /// <summary>
    /// Returns true if user has Developer role.
    /// </summary>
    public bool IsDeveloper => Role == "Developer";
    
    /// <summary>
    /// Returns true if user has Admin role.
    /// </summary>
    public bool IsAdmin => Role == "Admin";
    
    /// <summary>
    /// Returns true if user has Moderator role.
    /// </summary>
    public bool IsModerator => Role == "Moderator";
    
    /// <summary>
    /// Returns true if user has User role (normal user).
    /// </summary>
    public bool IsUser => Role == "User";
    
    // ============================================
    // PERMISSION CHECK PROPERTIES (Computed)
    // ============================================
    
    /// <summary>
    /// Returns true if user can manage (create, edit, disable) other users.
    /// Only Developer and Admin can manage users.
    /// </summary>
    public bool CanManageUsers => IsDeveloper || IsAdmin;
    
    /// <summary>
    /// Returns true if user can manage stock (add products, edit products).
    /// Developer, Admin, and Moderator can manage stock.
    /// </summary>
    public bool CanManageStock => IsDeveloper || IsAdmin || IsModerator;
    
    /// <summary>
    /// Returns true if user can use the attendance tracker.
    /// Developer, Admin, and Moderator can use attendance tracker.
    /// </summary>
    public bool CanUseAttendanceTracker => IsDeveloper || IsAdmin || IsModerator;
    
    /// <summary>
    /// Returns true if user can restock products (add inventory).
    /// Developer, Admin, and Moderator can restock.
    /// </summary>
    public bool CanRestock => IsDeveloper || IsAdmin || IsModerator;
}

