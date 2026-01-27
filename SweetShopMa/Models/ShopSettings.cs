using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Global business configuration settings. 
/// This table will always contain a single row with Id = 1.
/// </summary>
public class ShopSettings
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    // Global Business Information
    public string BusinessName { get; set; } = "Sweet Shop";
    public string BusinessNameArabic { get; set; } = "متجر حلويات";
    public string TaxNumber { get; set; } = "";
    public string Currency { get; set; } = "EGP";

    // Active Session Info
    public int? ActiveLocationId { get; set; }

    // Receipt Customization
    public string ReceiptFooter { get; set; } = "Thank you for shopping with us!";
    public string ReceiptFooterArabic { get; set; } = "شكراً لتسوقكم معنا!";

    // Audit Info
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;
}
