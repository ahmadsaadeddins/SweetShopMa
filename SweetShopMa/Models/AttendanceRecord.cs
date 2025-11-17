using SQLite;

namespace SweetShopMa.Models;

public class AttendanceRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Status { get; set; } = "Present";
    public bool IsPresent { get; set; } = true;
    public decimal RegularHours { get; set; } = 0;
    public decimal OvertimeHours { get; set; } = 0;
    public decimal DailyPay { get; set; } = 0;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public decimal TotalHours => RegularHours + OvertimeHours;

    [Ignore]
    public string CheckInDisplay => CheckInTime?.ToString("HH:mm") ?? "--";

    [Ignore]
    public string CheckOutDisplay => CheckOutTime?.ToString("HH:mm") ?? "--";
}

