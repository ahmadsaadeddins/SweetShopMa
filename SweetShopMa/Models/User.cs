using SQLite;

namespace SweetShopMa.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; } // In production, this should be hashed
    public string Role { get; set; } = "User"; // Developer, Admin, Moderator, User
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsEnabled { get; set; } = true;
    public decimal MonthlySalary { get; set; } = 0m;
    
    // Role checks
    public bool IsDeveloper => Role == "Developer";
    public bool IsAdmin => Role == "Admin";
    public bool IsModerator => Role == "Moderator";
    public bool IsUser => Role == "User";
    
    // Permission checks
    public bool CanManageUsers => IsDeveloper || IsAdmin;
    public bool CanManageStock => IsDeveloper || IsAdmin || IsModerator;
    public bool CanUseAttendanceTracker => IsDeveloper || IsAdmin || IsModerator;
    public bool CanRestock => IsDeveloper || IsAdmin || IsModerator;
}

