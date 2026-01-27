using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Junction table for assigning users to locations.
/// </summary>
public class UserLocation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    [Indexed]
    public int LocationId { get; set; }

    public bool IsPrimary { get; set; } = false;
    public DateTime AssignedDate { get; set; } = DateTime.Now;

    [Ignore]
    public User User { get; set; }

    [Ignore]
    public ShopLocation Location { get; set; }
}
