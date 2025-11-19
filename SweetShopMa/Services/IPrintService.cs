namespace SweetShopMa.Services;

/// <summary>
/// Interface for printing receipts across different platforms
/// </summary>
public interface IPrintService
{
    /// <summary>
    /// Prints a receipt with the given text content
    /// </summary>
    Task<bool> PrintReceiptAsync(string receiptText, string title);
}

