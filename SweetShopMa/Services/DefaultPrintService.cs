using Microsoft.Maui.ApplicationModel;

namespace SweetShopMa.Services;

/// <summary>
/// Default print service implementation that uses Share API as fallback
/// </summary>
public class DefaultPrintService : IPrintService
{
    public async Task<bool> PrintReceiptAsync(string receiptText, string title)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = receiptText,
                Title = title
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}

