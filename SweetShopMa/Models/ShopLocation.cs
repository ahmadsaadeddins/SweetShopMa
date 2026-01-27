using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents a physical branch or shop location.
/// </summary>
public class ShopLocation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string LocationName { get; set; } = "";
    public string LocationNameArabic { get; set; } = "";

    // Contact Details
    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";

    // Address
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "Egypt";

    // Flags
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;
}
