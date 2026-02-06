# SweetShopMa Code Protection - Implementation Guide

**Version:** 1.0  
**Date:** 2025-02-01  
**Protection Level:** Professional (Recommended)

---

## 🚀 Quick Start (5 Steps)

### Step 1: Add ConfuserEx Package

```bash
# Navigate to project directory
cd SweetShopMa

# Add ConfuserEx NuGet package
dotnet add package ConfuserEx
```

### Step 2: Create Obfuscation Configuration

Create file: `SweetShopMa/confuser.crproj`

```xml
<?xml version="1.0" encoding="utf-8"?>
<project baseDir=".\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish" 
         outputDir=".\Protected" 
         xmlns="http://confuser.codeplex.com">
  
  <module path="SweetShopMa.exe">
    <rule pattern="true" preset="normal" inherit="false">
      <!-- Anti-debugging -->
      <protection id="anti debug" />
      
      <!-- Anti-dump -->
      <protection id="anti dump" />
      
      <!-- Anti-tampering -->
      <protection id="anti tamper" />
      
      <!-- Constant encryption -->
      <protection id="constants" />
      
      <!-- Control flow obfuscation -->
      <protection id="ctrl flow" />
      
      <!-- Rename obfuscation -->
      <protection id="rename">
        <argument name="mode" value="decodable" />
      </protection>
      
      <!-- Resource encryption -->
      <protection id="resources" />
    </rule>
  </module>
</project>
```

### Step 3: Create Hardware Lock Service

Create file: `SweetShopMa/Services/LicensingService.cs`

```csharp
using System;
using System.Management;
using System.Security.Cryptography;
using System.IO;
using System.Text.Json;

namespace SweetShopMa.Services;

/// <summary>
/// Hardware-based licensing service to prevent unauthorized copying.
/// </summary>
public class LicensingService
{
    private const string LicenseFile = "license.dat";
    private const string InternalKey = "YOUR_INTERNAL_ENCRYPTION_KEY"; // Change this!
    
    /// <summary>
    /// Validates the license on app startup.
    /// Returns true if license is valid, false otherwise.
    /// </summary>
    public (bool IsValid, string Message) ValidateLicense()
    {
        try
        {
            // Get hardware ID
            var hwId = GetHardwareId();
            if (string.IsNullOrEmpty(hwId))
            {
                return (false, "Could not determine hardware ID.");
            }
            
            // Load license file
            if (!File.Exists(LicenseFile))
            {
                return (false, $"License file not found: {LicenseFile}");
            }
            
            var licenseJson = File.ReadAllText(LicenseFile);
            var license = JsonSerializer.Deserialize<License>(licenseJson);
            
            if (license == null)
            {
                return (false, "Invalid license file format.");
            }
            
            // Verify hardware ID matches
            if (license.HardwareId != hwId)
            {
                return (false, "License is not valid for this computer.");
            }
            
            // Verify signature
            if (!VerifySignature(license))
            {
                return (false, "License signature is invalid.");
            }
            
            // Check expiration
            if (license.ExpirationDate < DateTime.Now)
            {
                return (false, $"License expired on {license.ExpirationDate:yyyy-MM-dd}");
            }
            
            return (true, "License is valid.");
        }
        catch (Exception ex)
        {
            return (false, $"Error validating license: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Generates a license for a specific hardware ID.
    /// Call this method on YOUR computer to generate licenses for customers.
    /// </summary>
    public string GenerateLicense(string hardwareId, DateTime expirationDate, string[] features)
    {
        var license = new License
        {
            HardwareId = hardwareId,
            ExpirationDate = expirationDate,
            Features = features,
            GeneratedDate = DateTime.Now
        };
        
        // Generate signature
        license.Signature = GenerateSignature(license);
        
        return JsonSerializer.Serialize(license, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
    
    /// <summary>
    /// Gets the unique hardware ID for this computer.
    /// </summary>
    private string GetHardwareId()
    {
        try
        {
            var cpuId = GetCpuId();
            var motherboardId = GetMotherboardId();
            
            // Combine and hash
            var combined = $"{cpuId}|{motherboardId}";
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Gets the CPU ID.
    /// </summary>
    private string GetCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                return obj["ProcessorId"]?.ToString();
            }
        }
        catch { }
        return "UNKNOWN_CPU";
    }
    
    /// <summary>
    /// Gets the Motherboard ID.
    /// </summary>
    private string GetMotherboardId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString();
            }
        }
        catch { }
        return "UNKNOWN_MOTHERBOARD";
    }
    
    /// <summary>
    /// Generates a signature for the license.
    /// </summary>
    private string GenerateSignature(License license)
    {
        var data = $"{license.HardwareId}|{license.ExpirationDate:yyyyMMdd}|{InternalKey}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(bytes);
    }
    
    /// <summary>
    /// Verifies the license signature.
    /// </summary>
    private bool VerifySignature(License license)
    {
        var expectedSignature = GenerateSignature(license);
        return license.Signature == expectedSignature;
    }
    
    /// <summary>
    /// Gets the current hardware ID (for license generation).
    /// </summary>
    public string GetCurrentHardwareId()
    {
        return GetHardwareId();
    }
}

/// <summary>
/// License data structure.
/// </summary>
public class License
{
    public string HardwareId { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string[] Features { get; set; }
    public DateTime GeneratedDate { get; set; }
    public string Signature { get; set; }
}
```

### Step 4: Update MauiProgram.cs to Check License

```csharp
// MauiProgram.cs - Add at the beginning of CreateMauiApp()
public static MauiApp CreateMauiApp()
{
    // 🔒 STEP 1: Validate License FIRST
    var licensingService = new LicensingService();
    var (isValid, message) = licensingService.ValidateLicense();
    
    if (!isValid)
    {
        // Show error and exit
        Task.Run(async () =>
        {
            await Task.Delay(100); // Small delay for UI to initialize
            Application.Current?.MainPage?.DisplayAlert("License Error", message, "OK").ContinueWith(t =>
            {
                Application.Current?.Quit();
            });
        });
        
        // Continue anyway for development (remove this in production!)
        #if DEBUG
        System.Diagnostics.Debug.WriteLine($"License check failed (DEBUG mode): {message}");
        #else
        // In release mode, don't continue
        #endif
    }
    
    // ... rest of existing code
}
```

### Step 5: Build and Protect

```bash
# Step 5.1: Build Release version
dotnet build -c Release

# Step 5.2: Publish as self-contained
dotnet publish -c Release -r win10-x64 --self-contained

# Step 5.3: Run ConfuserEx
# Download from: https://github.com/mkaringer/ConfuserEx/releases
# Extract and run:
Confuser.CLI.exe -n confuser.crproj

# Step 5.4: Your protected app is in: SweetShopMa/Protected/
```

---

## 📝 License Generation Tool

Create file: `SweetShopMa/Tools/LicenseGenerator.cs`

```csharp
using System;
using SweetShopMa.Services;

namespace SweetShopMa.Tools;

/// <summary>
/// Tool to generate licenses for customers.
/// Run this on YOUR computer to create license files.
/// </summary>
public class LicenseGenerator
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== SweetShopMa License Generator ===\n");
        
        var licensing = new LicensingService();
        
        // Get customer's hardware ID
        Console.Write("Enter customer's Hardware ID: ");
        var hwId = Console.ReadLine();
        
        if (string.IsNullOrEmpty(hwId))
        {
            Console.WriteLine("Hardware ID cannot be empty!");
            return;
        }
        
        // Get expiration date
        Console.Write("Enter expiration date (YYYY-MM-DD) or press Enter for 1 year from now: ");
        var dateInput = Console.ReadLine();
        
        DateTime expirationDate;
        if (string.IsNullOrEmpty(dateInput))
        {
            expirationDate = DateTime.Now.AddYears(1);
        }
        else if (!DateTime.TryParse(dateInput, out expirationDate))
        {
            Console.WriteLine("Invalid date format!");
            return;
        }
        
        // Get features
        Console.WriteLine("Available features:");
        Console.WriteLine("  1. pos - Point of Sale");
        Console.WriteLine("  2. reports - Sales Reports");
        Console.WriteLine("  3. attendance - Attendance Tracking");
        Console.WriteLine("  4. admin - Admin Panel");
        Console.WriteLine("  5. all - All features");
        Console.Write("Enter features (comma-separated, e.g., 'pos,reports' or 'all'): ");
        
        var featuresInput = Console.ReadLine();
        string[] features;
        
        if (featuresInput?.ToLower() == "all")
        {
            features = new[] { "pos", "reports", "attendance", "admin" };
        }
        else
        {
            features = featuresInput?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(f => f.Trim().ToLower())
                           .ToArray() ?? new[] { "pos" };
        }
        
        // Generate license
        Console.WriteLine("\nGenerating license...");
        var licenseJson = licensing.GenerateLicense(hwId, expirationDate, features);
        
        // Save to file
        var filename = $"license_{hwId}.dat";
        File.WriteAllText(filename, licenseJson);
        
        Console.WriteLine($"\n✅ License saved to: {filename}");
        Console.WriteLine($"\nLicense Details:");
        Console.WriteLine($"  Hardware ID: {hwId}");
        Console.WriteLine($"  Expiration: {expirationDate:yyyy-MM-dd}");
        Console.WriteLine($"  Features: {string.Join(", ", features)}");
        Console.WriteLine($"\nSend this file to the customer and ask them to place it in:");
        Console.WriteLine($"  C:\\Users\\{{Username}}\\AppData\\Local\\SweetShopMa\\{filename}");
    }
}
```

---

## 🔐 Database Encryption Setup

### Step 1: Add SQLCipher Package

```bash
dotnet add package SQLitePCLRaw.bundle_e_sqlcipher
```

### Step 2: Update DatabaseService.cs

```csharp
// Services/DatabaseService.cs - Add encryption
public class DatabaseService
{
    private const string DbPassword = "YOUR_SECURE_DB_PASSWORD"; // Change this!
    
    private async Task InitializeAsync()
    {
        if (_database is not null)
            return;

        await _initSemaphore.WaitAsync();
        try
        {
            if (_database is not null)
                return;

            // 🔒 ENCRYPTED DATABASE CONNECTION
            var connectionString = new SQLiteConnectionString(
                DbPath, 
                true,  // Open in read-write mode
                key: DbPassword  // 🔑 ENCRYPTION KEY
            );
            
            _database = new SQLiteAsyncConnection(connectionString);
            
            // Enable encryption
            await _database.ExecuteAsync($"PRAGMA key = '{DbPassword}'");
            
            // Rest of existing code...
            await _database.CreateTableAsync<User>();
            // ... etc
        }
        finally
        {
            _initSemaphore.Release();
        }
    }
}
```

---

## 🛡️ Anti-Tampering Service

Create file: `SweetShopMa/Services/IntegrityService.cs`

```csharp
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.IO;

namespace SweetShopMa.Services;

/// <summary>
/// Service to verify application integrity and detect tampering.
/// </summary>
public class IntegrityService
{
    // This hash should be computed AFTER obfuscation and hardcoded here
    private const string ExpectedHash = "COMPUTE_THIS_AFTER_OBFUSCATION";
    
    /// <summary>
    /// Verifies the application hasn't been tampered with.
    /// </summary>
    public bool VerifyIntegrity()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var location = assembly.Location;
            
            if (!File.Exists(location))
            {
                return false;
            }
            
            var hash = ComputeHash(location);
            
            #if DEBUG
            // In debug mode, just log the hash for you to copy
            System.Diagnostics.Debug.WriteLine($"Assembly Hash: {hash}");
            return true; // Skip verification in debug
            #else
            return hash == ExpectedHash;
            #endif
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Computes SHA256 hash of a file.
    /// </summary>
    private string ComputeHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var bytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
    
    /// <summary>
    /// Gets the current assembly hash (call this after obfuscation).
    /// </summary>
    public string GetCurrentHash()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return ComputeHash(assembly.Location);
    }
}
```

### Update MauiProgram.cs to Check Integrity

```csharp
// MauiProgram.cs - Add after license check
public static MauiApp CreateMauiApp()
{
    // License check...
    
    // 🔒 STEP 2: Verify Integrity
    var integrityService = new IntegrityService();
    if (!integrityService.VerifyIntegrity())
    {
        Task.Run(async () =>
        {
            await Task.Delay(100);
            Application.Current?.MainPage?.DisplayAlert(
                "Security Error", 
                "Application has been tampered with!", 
                "OK"
            ).ContinueWith(t => Application.Current?.Quit());
        });
        
        #if DEBUG
        System.Diagnostics.Debug.WriteLine("Integrity check failed (DEBUG mode)");
        #endif
    }
    
    // ... rest of code
}
```

---

## 📦 Complete Build Script

Create file: `build-protected.bat`

```batch
@echo off
echo ========================================
echo SweetShopMa Protected Build Script
echo ========================================
echo.

echo Step 1: Cleaning...
dotnet clean
echo.

echo Step 2: Building Release...
dotnet build -c Release
if errorlevel 1 goto error
echo.

echo Step 3: Publishing...
dotnet publish -c Release -r win10-x64 --self-contained -p:PublishTrimmed=true
if errorlevel 1 goto error
echo.

echo Step 4: Obfuscating with ConfuserEx...
REM Make sure Confuser.CLI.exe is in the same directory or in PATH
Confuser.CLI.exe -n confuser.crproj
if errorlevel 1 goto error
echo.

echo ========================================
echo SUCCESS! Protected app is in:
echo   SweetShopMa\Protected\
echo ========================================
goto end

:error
echo.
echo ========================================
echo BUILD FAILED!
echo ========================================
pause
exit /b 1

:end
pause
```

---

## 🎯 Deployment Checklist

- [ ] 1. Build Release version
- [ ] 2. Generate license for customer's hardware ID
- [ ] 3. Create protected executable with ConfuserEx
- [ ] 4. Test protected executable
- [ ] 5. Verify database encryption works
- [ ] 6. Verify license check works
- [ ] 7. Verify integrity check works
- [ ] 8. Package for distribution:
  - [ ] SweetShopMa.exe (protected)
  - [ ] license.dat (customer-specific)
  - [ ] README.txt (installation instructions)
  - [ ] Run installer if needed

---

## 📖 Installation Instructions for Customers

Create file: `README_INSTALL.txt`

```
SweetShopMa - Installation Instructions
========================================

1. Copy SweetShopMa.exe to any folder on your computer.

2. Copy the license.dat file to the same folder.

3. Run SweetShopMa.exe.

4. The application will:
   - Verify your license
   - Create an encrypted database
   - Start the application

TROUBLESHOOTING:
===============

Q: "License is not valid for this computer"
A: The license file is tied to your computer. Contact support for a new license.

Q: "License expired"
A: Your license has expired. Contact support to renew.

Q: Application won't start
A: Make sure license.dat is in the same folder as SweetShopMa.exe.

SUPPORT:
========
Email: support@sweetshop.com
Phone: +20 XXX XXX XXXX
```

---

## 🔑 Security Best Practices

1. **Never commit your internal encryption keys** to version control
2. **Use different keys for each customer** if possible
3. **Keep a backup of license files** you generate
4. **Set reasonable expiration dates** (e.g., 1 year)
5. **Provide trial licenses** for potential customers
6. **Monitor for abuse** (e.g., same HWID requesting multiple licenses)

---

**End of Implementation Guide**

*For questions or issues, refer to:*
- *ConfuserEx Documentation: https://github.com/mkaringer/ConfuserEx*
- *SQLCipher Documentation: https://www.zetetic.net/sqlcipher/*
- *.NET MAUI Documentation: https://learn.microsoft.com/en-us/dotnet/maui/*
