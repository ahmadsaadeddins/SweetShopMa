#if WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.Services;

namespace SweetShopMa.Platforms.Windows;

/// <summary>
/// Windows-specific implementation of cash drawer service.
/// 
/// WHAT IS WINDOWSCASHDRAWERSERVICE?
/// This service opens cash drawers connected to receipt printers using ESC/POS commands.
/// Cash drawers are typically connected to receipt printers via a cable, and the
/// printer can open the drawer when it receives a specific command.
/// 
/// HOW CASH DRAWERS WORK:
/// Most receipt printers (especially thermal printers) have a cash drawer port.
/// When the printer receives an ESC/POS command (ESC p 0 25 250), it sends a signal
/// to the connected cash drawer to open.
/// 
/// ESC/POS COMMAND:
/// - ESC = 0x1B (escape character)
/// - p = 0x70 (command to open drawer)
/// - 0 = 0x00 (drawer pin number, usually 0)
/// - 25 = 0x19 (pulse on time in milliseconds)
/// - 250 = 0xFA (pulse off time in milliseconds)
/// 
/// METHODS USED:
/// 1. Direct serial/parallel port communication (if printer is on COM/LPT port)
/// 2. Raw print command via Windows "copy" command (sends raw bytes to printer)
/// 
/// LIMITATIONS:
/// - Only works on Windows (uses Windows-specific APIs)
/// - Requires printer to support ESC/POS commands
/// - Cash drawer must be connected to printer
/// </summary>
public class WindowsCashDrawerService : ICashDrawerService
{
    /// <summary>
    /// Opens the cash drawer by sending ESC/POS command to the printer.
    /// 
    /// HOW IT WORKS:
    /// 1. Ensure we're on the main thread
    /// 2. Create ESC/POS command bytes: ESC p 0 25 250
    /// 3. Try Method 1: Find printer port (COM/LPT) and send command directly
    /// 4. If Method 1 fails, try Method 2: Send raw print command to default printer
    /// 5. Return true if either method succeeds
    /// 
    /// THREAD SAFETY:
    /// If called from background thread, switches to main thread automatically.
    /// </summary>
    /// <returns>True if drawer command was sent successfully, false otherwise</returns>
    public async Task<bool> OpenDrawerAsync()
    {
        try
        {
            // Ensure we're on the main thread
            if (!MainThread.IsMainThread)
            {
                return await MainThread.InvokeOnMainThreadAsync(async () => await OpenDrawerAsync());
            }

            // ESC/POS command to open cash drawer: ESC p 0 25 250
            // ESC = 0x1B, p = 0x70, 0 = 0x00, 25 = 0x19, 250 = 0xFA
            byte[] drawerCommand = { 0x1B, 0x70, 0x00, 0x19, 0xFA };

            // Method 1: Try to find printer port and send command directly
            var printerPort = await GetPrinterPortAsync();
            if (!string.IsNullOrEmpty(printerPort))
            {
                if (await SendCommandToPortAsync(printerPort, drawerCommand))
                {
                    return true;
                }
            }

            // Method 2: Try using Windows RAW print to default printer
            return await SendRawPrintCommandAsync(drawerCommand);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cash drawer error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    private async Task<string> GetPrinterPortAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = TRUE"))
                {
                    foreach (ManagementObject printer in searcher.Get())
                    {
                        var portName = printer["PortName"]?.ToString();
                        if (!string.IsNullOrEmpty(portName))
                        {
                            // Check if it's a COM or LPT port
                            if (portName.StartsWith("COM") || portName.StartsWith("LPT"))
                            {
                                return portName;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
            return null;
        });
    }

    private async Task<bool> SendCommandToPortAsync(string portName, byte[] command)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (portName.StartsWith("COM"))
                {
                    // Serial port
                    using (var port = new SerialPort(portName, 9600))
                    {
                        port.Open();
                        port.Write(command, 0, command.Length);
                        port.Close();
                        return true;
                    }
                }
                else if (portName.StartsWith("LPT"))
                {
                    // Parallel port - use file write
                    try
                    {
                        using (var stream = new FileStream(portName, FileMode.Open, FileAccess.Write))
                        {
                            stream.Write(command, 0, command.Length);
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
            return false;
        });
    }

    private async Task<bool> SendRawPrintCommandAsync(byte[] command)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Get default printer name
                string printerName = null;
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = TRUE"))
                {
                    foreach (ManagementObject printer in searcher.Get())
                    {
                        printerName = printer["Name"]?.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(printerName))
                {
                    System.Diagnostics.Debug.WriteLine("No default printer found");
                    return false;
                }

                // Create a temporary file with the raw command
                var tempFile = Path.Combine(Path.GetTempPath(), $"drawer_{Guid.NewGuid()}.raw");
                File.WriteAllBytes(tempFile, command);

                try
                {
                    // Use Windows copy command to send raw data to printer
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c copy /b \"{tempFile}\" \"{printerName}\"",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    var process = Process.Start(processStartInfo);
                    if (process != null)
                    {
                        process.WaitForExit(2000); // Wait max 2 seconds
                        
                        // Clean up
                        try
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                            }
                        }
                        catch { }

                        return process.ExitCode == 0;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error sending raw print: {ex.Message}");
                    try
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                    }
                    catch { }
                }

                return false;
            }
            catch
            {
                return false;
            }
        });
    }
}
#endif

