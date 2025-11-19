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

public class WindowsCashDrawerService : ICashDrawerService
{
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

