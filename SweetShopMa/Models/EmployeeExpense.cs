using SQLite;

namespace SweetShopMa.Models;

public class EmployeeExpense
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = "General";
    public string Notes { get; set; } = "";
    public DateTime ExpenseDate { get; set; } = DateTime.Today;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

