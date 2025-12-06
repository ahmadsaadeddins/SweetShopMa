namespace SweetShopMa.Utils;

/// <summary>
/// Application-wide constants to avoid magic numbers and hardcoded values.
/// </summary>
public static class AppConstants
{
    // ============================================
    // PASSWORD HASHING
    // ============================================
    
    /// <summary>
    /// Number of iterations for PBKDF2 password hashing.
    /// Higher values increase security but also computation time.
    /// </summary>
    public const int PasswordHashIterations = 100000;
    
    // ============================================
    // UI TIMING
    // ============================================
    
    /// <summary>
    /// Timeout in milliseconds for auto-hiding notification messages.
    /// </summary>
    public const int NotificationTimeoutMs = 2000;
    
    /// <summary>
    /// Delay in milliseconds before navigating to ensure Shell is ready.
    /// </summary>
    public const int NavigationDelayMs = 200;
    
    /// <summary>
    /// Delay in milliseconds for error handling flag reset.
    /// </summary>
    public const int ErrorHandlingDelayMs = 200;
    
    /// <summary>
    /// Delay in milliseconds for post-checkout state reset.
    /// </summary>
    public const int PostCheckoutResetDelayMs = 1000;
    
    /// <summary>
    /// Delay in milliseconds for focus operations.
    /// </summary>
    public const int FocusDelayMs = 100;
    
    /// <summary>
    /// Delay in milliseconds for unfocus operations.
    /// </summary>
    public const int UnfocusDelayMs = 50;
    
    // ============================================
    // BUSINESS RULES
    // ============================================
    
    /// <summary>
    /// Default overtime multiplier (time and a half).
    /// </summary>
    public const decimal DefaultOvertimeMultiplier = 1.5m;
    
    /// <summary>
    /// Default monthly salary for new users.
    /// </summary>
    public const decimal DefaultMonthlySalary = 0m;
    
    /// <summary>
    /// Minimum password length for user accounts.
    /// </summary>
    public const int MinimumPasswordLength = 4;
    
    // ============================================
    // DATABASE
    // ============================================
    
    /// <summary>
    /// Database filename.
    /// </summary>
    public const string DatabaseFileName = "sweetshop.db3";
    
    // ============================================
    // DEFAULT VALUES
    // ============================================
    
    /// <summary>
    /// Default quantity for unit-based products.
    /// </summary>
    public const decimal DefaultUnitQuantity = 1m;
    
    /// <summary>
    /// Default quantity for weight-based products (in kilos).
    /// </summary>
    public const decimal DefaultWeightQuantity = 0.5m;
    
    /// <summary>
    /// Default work start time (8:00 AM).
    /// </summary>
    public const int DefaultWorkStartHour = 8;
    
    /// <summary>
    /// Default work end time (4:00 PM / 16:00).
    /// </summary>
    public const int DefaultWorkEndHour = 16;
    
    /// <summary>
    /// Standard work hours per day.
    /// </summary>
    public const int StandardWorkHoursPerDay = 8;
}

