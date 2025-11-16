using SQLite;

namespace SweetShopMa.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; } // In production, this should be hashed
    public string Role { get; set; } = "Customer"; // Admin, Customer
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public bool IsAdmin => Role == "Admin";
}

