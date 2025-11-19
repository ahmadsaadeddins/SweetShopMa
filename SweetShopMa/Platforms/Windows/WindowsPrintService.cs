#if WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.Services;

namespace SweetShopMa.Platforms.Windows;

public class WindowsPrintService : IPrintService
{
    public async Task<bool> PrintReceiptAsync(string receiptText, string title)
    {
        try
        {
            // Ensure we're on the main thread
            if (!MainThread.IsMainThread)
            {
                return await MainThread.InvokeOnMainThreadAsync(async () => await PrintReceiptAsync(receiptText, title));
            }

            // Create a temporary HTML file for printing
            var tempPath = Path.Combine(Path.GetTempPath(), $"receipt_{Guid.NewGuid()}.html");
            
            // Create HTML content with proper formatting
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{title}</title>
    <style>
        @media print {{
            @page {{
                margin: 0.5in;
                size: letter;
            }}
        }}
        body {{
            font-family: 'Courier New', Consolas, monospace;
            font-size: 12pt;
            line-height: 1.4;
            white-space: pre-wrap;
            margin: 0;
            padding: 20px;
        }}
    </style>
</head>
<body>
{WebUtility.HtmlEncode(receiptText)}
</body>
</html>";

            // Add JavaScript to auto-trigger print dialog when page loads
            htmlContent = htmlContent.Replace("</body>", @"
<script>
window.onload = function() {
    setTimeout(function() {
        window.print();
    }, 250);
};
</script>
</body>");

            // Write HTML file
            await File.WriteAllTextAsync(tempPath, htmlContent, Encoding.UTF8);

            // Open the HTML file in default browser - it will auto-trigger print dialog
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                var process = Process.Start(processStartInfo);
                
                if (process == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to start process");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }

            // Clean up file after a longer delay (give time for print dialog and printing)
            _ = Task.Run(async () =>
            {
                await Task.Delay(30000); // Wait 30 seconds to allow printing
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch
                {
                    // Ignore cleanup errors - file might still be in use
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
#endif

