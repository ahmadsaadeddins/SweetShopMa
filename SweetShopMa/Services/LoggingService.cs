using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace SweetShopMa.Services;

/// <summary>
/// Centralized logging service for the application.
/// Provides methods for logging errors, warnings, and info messages.
/// </summary>
public class LoggingService
{
    private const string LogFileName = "app_log.txt";
    private const int MaxLogFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private readonly object _logLock = new();

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    public void LogError(string context, Exception ex, string additionalInfo = null)
    {
        var message = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {context}";
        if (!string.IsNullOrEmpty(additionalInfo))
            message += $" | {additionalInfo}";

        message += $"\n  Exception: {ex.Message}";
        if (ex.InnerException != null)
            message += $"\n  Inner: {ex.InnerException.Message}";
        message += $"\n  StackTrace: {ex.StackTrace}\n";

        WriteLog(message);
        System.Diagnostics.Debug.WriteLine(message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void LogWarning(string context, string message)
    {
        var logMessage = $"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {context}: {message}";
        WriteLog(logMessage);
        System.Diagnostics.Debug.WriteLine(logMessage);
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void LogInfo(string context, string message)
    {
        var logMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {context}: {message}";
        WriteLog(logMessage);
        System.Diagnostics.Debug.WriteLine(logMessage);
    }

    /// <summary>
    /// Writes log message to file.
    /// </summary>
    private void WriteLog(string message)
    {
        try
        {
            lock (_logLock)
            {
                var logPath = Path.Combine(FileSystem.AppDataDirectory, LogFileName);

                // Check file size and rotate if needed
                if (File.Exists(logPath))
                {
                    var fileInfo = new FileInfo(logPath);
                    if (fileInfo.Length > MaxLogFileSizeBytes)
                    {
                        // Archive old log
                        var archivePath = Path.Combine(FileSystem.AppDataDirectory, $"app_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                        File.Move(logPath, archivePath);
                    }
                }

                File.AppendAllText(logPath, message + Environment.NewLine);
            }
        }
        catch
        {
            // Silently fail if we can't write to log file
            // Don't want logging to crash the app
        }
    }

    /// <summary>
    /// Gets the current log file path.
    /// </summary>
    public string GetLogFilePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, LogFileName);
    }

    /// <summary>
    /// Clears the current log file.
    /// </summary>
    public void ClearLogs()
    {
        try
        {
            var logPath = GetLogFilePath();
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing logs: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the log file contents as a string.
    /// </summary>
    public async Task<string> GetLogsAsync()
    {
        try
        {
            var logPath = GetLogFilePath();
            if (File.Exists(logPath))
            {
                return await File.ReadAllTextAsync(logPath);
            }
            return "No logs available.";
        }
        catch (Exception ex)
        {
            return $"Error reading logs: {ex.Message}";
        }
    }
}
