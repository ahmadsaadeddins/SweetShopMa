# Lesson 7: Platform-Specific Code Made Simple

## 🎯 Learning Objectives

By the end of this lesson, you will:
- ✅ Understand why different platforms need different code
- ✅ Learn when to use platform-specific code vs. cross-platform code
- ✅ Master conditional compilation (#if ANDROID, #if IOS, etc.)
- ✅ Create platform-specific implementations with interfaces
- ✅ Use dependency injection for platform services
- ✅ Handle platform differences gracefully
- ✅ Access platform-specific features like biometrics, haptics, and file pickers

---

## 📖 Why Do We Need Platform-Specific Code?

### 🌍 The Real World: Different Countries, Different Rules

Think of MAUI like a **universal translator** that helps you speak to people from different countries:

- **Android** speaks Japanese 🇯🇵
- **iOS** speaks French 🇫🇷
- **Windows** speaks German 🇩🇪
- **macOS** speaks Spanish 🇪🇸

MAUI provides a **common language** (like English) that works everywhere. But sometimes you need to:
- Say something in the native language (platform-specific feature)
- Follow local customs (platform-specific behavior)
- Use local services (platform-specific APIs)

### 📱 Why Platforms Are Different

Each platform (Android, iOS, Windows, macOS) has:

| Aspect | What It Means | Example |
|--------|---------------|---------|
| **Different File Systems** | Where files are stored | Android: `/storage/emulated/0/` vs Windows: `C:\Users\` |
| **Different Permissions** | How users grant access | Android: Manifest vs iOS: Info.plist |
| **Different UI Patterns** | How users navigate | Android: Back button vs iOS: Swipe gesture |
| **Different Hardware** | Physical capabilities | Windows: Physical keyboard vs Mobile: Software keyboard |
| **Different APIs** | Available features | iOS: Face ID vs Android: Fingerprint |
| **Different Security** | How apps are sandboxed | iOS: Strict sandbox vs Android: More flexible |

### 🎯 The MAUI Promise

MAUI provides **cross-platform APIs** for common tasks:

```csharp
// ✅ This works on ALL platforms!
var button = new Button { Text = "Click Me" };
await DisplayAlert("Hello", "This works everywhere!", "OK");

// ✅ This works on ALL platforms!
var path = FileSystem.AppDataDirectory;  // MAUI handles the differences
```

**But sometimes you need platform-specific code:**

```csharp
// ❌ MAUI doesn't provide this cross-platform
// Need platform-specific code for biometric authentication
// Need platform-specific code for advanced file pickers
// Need platform-specific code for haptic feedback
```

---

## 🤔 When Should You Use Platform-Specific Code?

### ✅ USE Platform-Specific Code When:

1. **Accessing Platform-Specific Features**
   - Biometric authentication (Face ID, Fingerprint)
   - Haptic feedback (vibration patterns)
   - Advanced file pickers
   - Platform-specific UI controls
   - Native notifications
   - Battery information
   - Network status details

2. **Handling Platform Differences**
   - File path differences
   - Permission handling
   - Storage locations
   - Screen sizes and densities

3. **Optimizing for Platform**
   - Platform-specific performance optimizations
   - Native look and feel
   - Platform-specific user expectations

### ❌ AVOID Platform-Specific Code When:

1. **MAUI Provides Cross-Platform Alternative**
   ```csharp
   // ❌ BAD: Writing platform-specific file code
   #if ANDROID
       var path = "/storage/emulated/0/data.db";
   #elif IOS
       var path = "/var/mobile/data.db";
   #endif
   
   // ✅ GOOD: Use MAUI's cross-platform API
   var path = Path.Combine(FileSystem.AppDataDirectory, "data.db");
   ```

2. **Business Logic is Platform-Independent**
   ```csharp
   // ❌ BAD: Platform-specific business logic
   #if ANDROID
       discount = price * 0.1;
   #elif IOS
       discount = price * 0.15;
   #endif
   
   // ✅ GOOD: Same logic for all platforms
   discount = price * discountRate;
   ```

3. **Can Use Abstraction Instead**
   ```csharp
   // ❌ BAD: Platform checks everywhere
   #if ANDROID
       ShowAndroidToast();
   #elif IOS
       ShowIOSAlert();
   #endif
   
   // ✅ GOOD: Use interface abstraction
   _notificationService.ShowMessage("Hello!");
   ```

---

## 🔧 Technique 1: Conditional Compilation

The simplest way to write platform-specific code is using **preprocessor directives**.

### 📝 What Are Preprocessor Directives?

They're instructions to the compiler: "Only compile this code for Android" or "Skip this code on iOS."

### Platform Constants Available in MAUI

```csharp
#if ANDROID     // Android platform
#elif IOS       // iOS platform
#elif WINDOWS   // Windows platform
#elif MACCATALYST // macOS platform (Mac Catalyst)
#elif TIZEN     // Tizen platform
#else           // Fallback for other platforms
#endif
```

### 🎯 Basic Example: Platform-Specific Message

```csharp
public void ShowPlatformMessage()
{
    string message;
    
#if ANDROID
    message = "Running on Android! 🤖";
#elif IOS
    message = "Running on iOS! 🍎";
#elif WINDOWS
    message = "Running on Windows! 🪟";
#elif MACCATALYST
    message = "Running on macOS! 🐧";
#else
    message = "Running on unknown platform! ❓";
#endif
    
    Console.WriteLine(message);
}
```

**What happens?**
- On Android: Only the ANDROID code is compiled
- On iOS: Only the IOS code is compiled
- Other platforms' code is completely ignored

### 📁 Practical Example: Platform-Specific File Paths

```csharp
public class FileService
{
    public string GetDatabasePath()
    {
#if ANDROID
        // Android uses internal storage
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
            "app.db"
        );
        
#elif IOS
        // iOS uses Library directory (backed up)
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "..", 
            "Library", 
            "app.db"
        );
        
#elif WINDOWS
        // Windows uses LocalAppData
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "YourAppName", 
            "app.db"
        );
        
#elif MACCATALYST
        // macOS uses Application Support directory
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "app.db"
        );
        
#else
        throw new PlatformNotSupportedException("This platform is not supported");
#endif
    }
}
```

**⚠️ Better Approach:** Use MAUI's cross-platform APIs when possible!

```csharp
// ✅ BETTER: Use MAUI's FileSystem API
public string GetDatabasePath()
{
    return Path.Combine(FileSystem.AppDataDirectory, "app.db");
}
```

### 🔀 Combining Conditions

You can combine platform checks:

```csharp
#if ANDROID || IOS
    // Mobile-specific code
    Console.WriteLine("Mobile device - use touch UI");
#elif WINDOWS || MACCATALYST
    // Desktop-specific code
    Console.WriteLine("Desktop device - use mouse/keyboard UI");
#endif
```

### 🐛 Debug vs Release Builds

```csharp
public void LogMessage(string message)
{
#if DEBUG
    // Verbose logging in debug mode
    Console.WriteLine($"[DEBUG] {DateTime.Now}: {message}");
    Console.WriteLine($"Stack trace: {Environment.StackTrace}");
#else
    // Minimal logging in release mode
    Console.WriteLine(message);
#endif
}
```

---

## 🎯 Technique 2: Abstraction with Interfaces (The Better Way)

**This is the PROFESSIONAL approach!** Instead of scattering `#if` statements everywhere, create interfaces and provide platform-specific implementations.

### 🏗️ The Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Your ViewModel (Cross-Platform)                            │
│     └─> Uses IPlatformService interface                     │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  IPlatformService (Interface - The Contract)                │
│     └─> Defines what methods are available                  │
└─────────────────────────────────────────────────────────────┘
                          ↓
        ┌─────────────────┼─────────────────┐
        ↓                 ↓                 ↓
┌───────────────┐ ┌───────────────┐ ┌───────────────┐
│  Android      │ │  iOS          │ │  Windows      │
│  Implementation│ │  Implementation│ │  Implementation│
└───────────────┘ └───────────────┘ └───────────────┘
```

**Benefits:**
- ✅ Clean, testable code
- ✅ No `#if` statements in business logic
- ✅ Easy to add new platforms
- ✅ Can mock for testing

---

### Step 1: Define the Interface

Create a **contract** that all platform implementations must follow:

```csharp
// Services/IPlatformService.cs
namespace MyApp.Services;

/// <summary>
/// Platform-specific operations that work differently on each platform
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// Gets the name of the current platform
    /// </summary>
    string GetPlatformName();
    
    /// <summary>
    /// Gets the app data directory for the current platform
    /// </summary>
    string GetAppDataPath();
    
    /// <summary>
    /// Shows a toast notification (platform-specific implementation)
    /// </summary>
    void ShowToast(string message);
    
    /// <summary>
    /// Vibrates the device (if supported)
    /// </summary>
    void Vibrate(int durationMs);
    
    /// <summary>
    /// Checks if biometric authentication is available
    /// </summary>
    bool IsBiometricAvailable();
}
```

---

### Step 2: Create Platform Implementations

Each platform gets its own implementation in the **Platforms** folder:

#### 🤖 Android Implementation

```csharp
// Platforms/Android/AndroidPlatformService.cs
using Android.Widget;
using Android.OS;
using MyApp.Services;

[assembly: Dependency(typeof(AndroidPlatformService))]

namespace MyApp.Platforms.Android;

public class AndroidPlatformService : IPlatformService
{
    public string GetPlatformName() => "Android";
    
    public string GetAppDataPath()
    {
        var context = Android.App.Application.Context;
        var filesDir = context.FilesDir?.AbsolutePath;
        return filesDir ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
    
    public void ShowToast(string message)
    {
        // Android uses native Toast
        var context = Android.App.Application.Context;
        Toast.MakeText(context, message, ToastLength.Short)?.Show();
    }
    
    public void Vibrate(int durationMs)
    {
        // Android vibration
        var vibrator = (Vibrator)Android.App.Application.Context
            ?.GetSystemService(Android.App.Context.VibratorService);
        
        if (vibrator == null) return;
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
        {
            // Android 12+ uses VibrationEffect
            var effect = VibrationEffect.CreateOneShot(
                durationMs, 
                VibrationEffect.DefaultAmplitude
            );
            vibrator.Vibrate(effect);
        }
        else
        {
            // Older Android versions
#pragma warning disable CS0618 // Type or member is obsolete
            vibrator.Vibrate(durationMs);
#pragma warning restore CS0618
        }
    }
    
    public bool IsBiometricAvailable()
    {
        var context = Android.App.Application.Context;
        return AndroidX.Core.Hardware.Fingerprint.FingerprintManagerCompat
            .From(context)
            ?.IsHardwareDetected ?? false;
    }
}
```

#### 🍎 iOS Implementation

```csharp
// Platforms/iOS/iOSPlatformService.cs
using Foundation;
using UIKit;
using AudioToolbox;
using LocalAuthentication;
using MyApp.Services;

[assembly: Dependency(typeof(iOSPlatformService))]

namespace MyApp.Platforms.iOS;

public class iOSPlatformService : IPlatformService
{
    public string GetPlatformName() => "iOS";
    
    public string GetAppDataPath()
    {
        // iOS uses Library directory (backed up by iTunes)
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documents, "..", "Library");
    }
    
    public void ShowToast(string message)
    {
        // iOS doesn't have native Toast, use UIAlertController
        var alert = UIAlertController.Create(
            null, 
            message, 
            UIAlertControllerStyle.Alert
        );
        
        var okAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, _ => { });
        alert.AddAction(okAction);
        
        // Get the top view controller
        var window = UIApplication.SharedApplication.KeyWindow;
        var rootViewController = window?.RootViewController;
        var topViewController = rootViewController;
        
        while (topViewController?.PresentedViewController != null)
        {
            topViewController = topViewController.PresentedViewController;
        }
        
        topViewController?.PresentViewController(alert, true, null);
    }
    
    public void Vibrate(int durationMs)
    {
        // iOS uses system sound for vibration
        SystemSound.Vibrate.PlaySystemSound();
    }
    
    public bool IsBiometricAvailable()
    {
        var context = new LAContext();
        var error = new NSError();
        return context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out error);
    }
}
```

#### 🪟 Windows Implementation

```csharp
// Platforms/Windows/WindowsPlatformService.cs
using Windows.UI.Xaml;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using MyApp.Services;

[assembly: Dependency(typeof(WindowsPlatformService))]

namespace MyApp.Platforms.Windows;

public class WindowsPlatformService : IPlatformService
{
    public string GetPlatformName() => "Windows";
    
    public string GetAppDataPath()
    {
        return Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData
        );
    }
    
    public void ShowToast(string message)
    {
        // Windows uses native Toast notifications
        var toastXml = $@"
            <toast>
                <visual>
                    <binding template='ToastGeneric'>
                        <text>{System.Security.SecurityElement.Escape(message)}</text>
                    </binding>
                </visual>
            </toast>";
        
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(toastXml);
        
        var toast = new ToastNotification(xmlDoc);
        ToastNotificationManager.CreateToastNotifier().Show(toast);
    }
    
    public void Vibrate(int durationMs)
    {
        // Windows doesn't support vibration (desktop)
        Console.WriteLine("Vibration not supported on Windows");
    }
    
    public bool IsBiometricAvailable()
    {
        // Windows Hello support check would go here
        return false; // Simplified for example
    }
}
```

---

### Step 3: Register in MauiProgram.cs

Tell MAUI which implementation to use for each platform:

```csharp
// MauiProgram.cs
using Microsoft.Extensions.Logging;
using MyApp.Services;

namespace MyApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        
        // Register platform-specific service
#if ANDROID
        builder.Services.AddSingleton<IPlatformService, AndroidPlatformService>();
#elif IOS
        builder.Services.AddSingleton<IPlatformService, iOSPlatformService>();
#elif WINDOWS
        builder.Services.AddSingleton<IPlatformService, WindowsPlatformService>();
#elif MACCATALYST
        builder.Services.AddSingleton<IPlatformService, MacPlatformService>();
#endif
        
        // Register other services and ViewModels...
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddTransient<MainPageViewModel>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        return builder.Build();
    }
}
```

**What happens?**
- On Android: Registers `AndroidPlatformService`
- On iOS: Registers `iOSPlatformService`
- On Windows: Registers `WindowsPlatformService`
- Your ViewModels don't need to know which platform!

---

### Step 4: Use in Your ViewModels (Cross-Platform!)

Now your ViewModels are completely cross-platform:

```csharp
// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Services;

namespace MyApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IPlatformService _platform;
    
    // DI injects the correct platform implementation automatically!
    public MainViewModel(IPlatformService platform)
    {
        _platform = platform;
        LoadPlatformInfo();
    }
    
    [ObservableProperty]
    private string _platformName;
    
    [ObservableProperty]
    private string _appDataPath;
    
    [ObservableProperty]
    private bool _biometricAvailable;
    
    private void LoadPlatformInfo()
    {
        PlatformName = _platform.GetPlatformName();
        AppDataPath = _platform.GetAppDataPath();
        BiometricAvailable = _platform.IsBiometricAvailable();
    }
    
    [RelayCommand]
    private void ShowNotification()
    {
        // Works on all platforms!
        _platform.ShowToast($"Hello from {_platform.GetPlatformName()}!");
    }
    
    [RelayCommand]
    private void VibrateDevice()
    {
        // Works on platforms that support it!
        _platform.Vibrate(500);
    }
}
```

**Notice:**
- ✅ NO `#if` statements in the ViewModel
- ✅ Clean, testable code
- ✅ Works on all platforms
- ✅ Easy to understand

---

## 🎨 Common Platform-Specific Features

Let's implement some common platform-specific features using the interface approach.

### 1️⃣ Biometric Authentication

Create a service for biometric authentication:

```csharp
// Services/IBiometricService.cs
public interface IBiometricService
{
    /// <summary>
    /// Checks if biometric authentication is available on this device
    /// </summary>
    bool IsAvailable();
    
    /// <summary>
    /// Authenticates the user using biometrics (Face ID, Fingerprint, etc.)
    /// </summary>
    /// <param name="reason">Reason shown to the user</param>
    /// <returns>True if authentication succeeded</returns>
    Task<bool> AuthenticateAsync(string reason);
}
```

**Android Implementation:**
```csharp
// Platforms/Android/AndroidBiometricService.cs
using AndroidX.Fragment.App;
using Android.Hardware.Biometrics;
using System.Threading.Tasks;

public class AndroidBiometricService : IBiometricService
{
    public bool IsAvailable()
    {
        var context = Android.App.Application.Context;
        return AndroidX.Core.Hardware.Fingerprint.FingerprintManagerCompat
            .From(context)
            ?.IsHardwareDetected ?? false;
    }
    
    public async Task<bool> AuthenticateAsync(string reason)
    {
        var activity = Platform.CurrentActivity as FragmentActivity;
        if (activity == null) return false;
        
        var promptInfo = new PromptInfo.Builder()
            .SetTitle("Authentication")
            .SetDescription(reason)
            .SetNegativeButtonText("Cancel")
            .Build();
        
        var callback = new BiometricAuthenticationCallback();
        var biometricPrompt = new BiometricPrompt(
            activity,
            null,
            callback
        );
        
        biometricPrompt.Authenticate(promptInfo);
        return await callback.Task;
    }
    
    private class BiometricAuthenticationCallback : BiometricPrompt.IAuthenticationCallback
    {
        private TaskCompletionSource<bool> _tcs = new();
        
        public Task<bool> Task => _tcs.Task;
        
        public void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
        {
            _tcs.TrySetResult(true);
        }
        
        public void OnAuthenticationFailed()
        {
            _tcs.TrySetResult(false);
        }
        
        public void OnAuthenticationError(int errorCode, ICharSequence errString)
        {
            _tcs.TrySetResult(false);
        }
    }
}
```

**iOS Implementation:**
```csharp
// Platforms/iOS/iOSBiometricService.cs
using LocalAuthentication;

public class iOSBiometricService : IBiometricService
{
    public bool IsAvailable()
    {
        var context = new LAContext();
        var error = new NSError();
        return context.CanEvaluatePolicy(
            LAPolicy.DeviceOwnerAuthenticationWithBiometrics, 
            out error
        );
    }
    
    public async Task<bool> AuthenticateAsync(string reason)
    {
        var context = new LAContext();
        var error = new NSError();
        
        var success = await context.EvaluatePolicyAsync(
            LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
            reason
        );
        
        return success;
    }
}
```

**Usage in ViewModel:**
```csharp
public class LoginViewModel : ObservableObject
{
    private readonly IBiometricService _biometric;
    
    public LoginViewModel(IBiometricService biometric)
    {
        _biometric = biometric;
    }
    
    [RelayCommand]
    private async Task BiometricLoginAsync()
    {
        if (!_biometric.IsAvailable())
        {
            await Shell.Current.DisplayAlert(
                "Not Available", 
                "Biometric authentication is not available on this device", 
                "OK"
            );
            return;
        }
        
        var success = await _biometric.AuthenticateAsync("Login to MyApp");
        
        if (success)
        {
            // Navigate to main page
            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            await Shell.Current.DisplayAlert(
                "Failed", 
                "Biometric authentication failed", 
                "OK"
            );
        }
    }
}
```

---

### 2️⃣ File Picker

```csharp
// Services/IFilePickerService.cs
public interface IFilePickerService
{
    Task<string> PickFileAsync(string[] allowedExtensions);
    Task<string> SaveFileAsync(string defaultName, string defaultExtension);
}
```

**Windows Implementation:**
```csharp
// Platforms/Windows/WindowsFilePickerService.cs
using Windows.Storage;
using Windows.Storage.Pickers;

public class WindowsFilePickerService : IFilePickerService
{
    public async Task<string> PickFileAsync(string[] allowedExtensions)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        
        foreach (var ext in allowedExtensions)
        {
            picker.FileTypeFilter.Add(ext.Replace(".", ""));
        }
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
    
    public async Task<string> SaveFileAsync(string defaultName, string defaultExtension)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = defaultName
        };
        
        picker.FileTypeChoices.Add(defaultExtension, new List<string> { defaultExtension });
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSaveFileAsync();
        return file?.Path;
    }
}
```

---

### 3️⃣ Battery Information

```csharp
// Services/IBatteryService.cs
public interface IBatteryService
{
    double GetChargeLevel();      // 0.0 to 1.0
    BatteryState GetState();      // Charging, Discharging, etc.
    PowerSource GetPowerSource(); // AC, USB, Wireless, Battery
}

public enum BatteryState
{
    Unknown,
    Charging,
    Discharging,
    Full,
    NotCharging
}

public enum PowerSource
{
    Battery,
    Ac,
    Usb,
    Wireless
}
```

**Android Implementation:**
```csharp
// Platforms/Android/AndroidBatteryService.cs
using Android.Content;
using Android.OS;

public class AndroidBatteryService : IBatteryService
{
    public double GetChargeLevel()
    {
        using var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Android.App.Application.Context.RegisterReceiver(null, filter);
        
        if (battery == null) return 0.0;
        
        int level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
        int scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);
        
        return scale > 0 ? (double)level / scale : 0.0;
    }
    
    public BatteryState GetState()
    {
        using var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Android.App.Application.Context.RegisterReceiver(null, filter);
        
        if (battery == null) return BatteryState.Unknown;
        
        var status = battery.GetIntExtra(BatteryManager.ExtraStatus, -1);
        
        return status switch
        {
            (int)BatteryStatus.Charging => BatteryState.Charging,
            (int)BatteryStatus.Discharging => BatteryState.Discharging,
            (int)BatteryStatus.Full => BatteryState.Full,
            (int)BatteryStatus.NotCharging => BatteryState.NotCharging,
            _ => BatteryState.Unknown
        };
    }
    
    public PowerSource GetPowerSource()
    {
        using var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Android.App.Application.Context.RegisterReceiver(null, filter);
        
        if (battery == null) return PowerSource.Battery;
        
        var chargePlug = battery.GetIntExtra(BatteryManager.ExtraPlugged, -1);
        
        return chargePlug switch
        {
            (int)BatteryPlugged.Usb => PowerSource.Usb,
            (int)BatteryPlugged.Ac => PowerSource.Ac,
            (int)BatteryPlugged.Wireless => PowerSource.Wireless,
            _ => PowerSource.Battery
        };
    }
}
```

---

## 🎯 Complete Example: Cross-Platform Logger

Let's build a logger that works differently on each platform but provides a unified interface.

### Interface

```csharp
// Services/ILogger.cs
public interface ILogger
{
    void Log(string message);
    void LogError(string error);
    void LogWarning(string warning);
}
```

### Android Implementation (Logcat)

```csharp
// Platforms/Android/AndroidLogger.cs
using Android.Util;

public class AndroidLogger : ILogger
{
    public void Log(string message)
    {
        Log.Debug("MyApp", message);
    }
    
    public void LogError(string error)
    {
        Log.Error("MyApp", error);
    }
    
    public void LogWarning(string warning)
    {
        Log.Warn("MyApp", warning);
    }
}
```

### iOS Implementation (Console)

```csharp
// Platforms/iOS/iOSLogger.cs
using Foundation;

public class iOSLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }
    
    public void LogError(string error)
    {
        Console.WriteLine($"[ERROR] {error}");
    }
    
    public void LogWarning(string warning)
    {
        Console.WriteLine($"[WARN] {warning}");
    }
}
```

### Windows Implementation (Debug Output)

```csharp
// Platforms/Windows/WindowsLogger.cs
using System.Diagnostics;

public class WindowsLogger : ILogger
{
    public void Log(string message)
    {
        Debug.WriteLine($"[INFO] {message}");
    }
    
    public void LogError(string error)
    {
        Debug.WriteLine($"[ERROR] {error}");
    }
    
    public void LogWarning(string warning)
    {
        Debug.WriteLine($"[WARN] {warning}");
    }
}
```

### Register in MauiProgram.cs

```csharp
#if ANDROID
    builder.Services.AddSingleton<ILogger, AndroidLogger>();
#elif IOS
    builder.Services.AddSingleton<ILogger, iOSLogger>();
#elif WINDOWS
    builder.Services.AddSingleton<ILogger, WindowsLogger>();
#endif
```

### Use in ViewModel

```csharp
public class MyViewModel : ObservableObject
{
    private readonly ILogger _logger;
    
    public MyViewModel(ILogger logger)
    {
        _logger = logger;
        _logger.Log("ViewModel initialized");
    }
    
    [RelayCommand]
    private void DoSomething()
    {
        try
        {
            _logger.Log("Doing something...");
            // Do work
            _logger.Log("Done!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
        }
    }
}
```

---

## 🐛 Common Mistakes & How to Avoid Them

### Mistake 1: Overusing Platform-Specific Code

```csharp
// ❌ BAD: Platform checks everywhere
public void SaveData(string data)
{
#if ANDROID
    File.WriteAllText("/storage/data.txt", data);
#elif IOS
    File.WriteAllText("/var/mobile/data.txt", data);
#endif
}

// ✅ GOOD: Use MAUI's cross-platform API
public void SaveData(string data)
{
    var path = Path.Combine(FileSystem.AppDataDirectory, "data.txt");
    File.WriteAllText(path, data);
}
```

### Mistake 2: Not Using Interfaces

```csharp
// ❌ BAD: Platform checks in ViewModel
public class MyViewModel
{
    public void ShowNotification()
    {
#if ANDROID
        ShowAndroidToast();
#elif IOS
        ShowIOSAlert();
#endif
    }
}

// ✅ GOOD: Use interface abstraction
public class MyViewModel
{
    private readonly INotificationService _notification;
    
    public MyViewModel(INotificationService notification)
    {
        _notification = notification;
    }
    
    public void ShowNotification()
    {
        _notification.Show("Hello!");
    }
}
```

### Mistake 3: Forgetting to Register Platform Services

```csharp
// ❌ ERROR: Service not registered
public MyViewModel(IPlatformService platform) { }

// ✅ FIX: Register in MauiProgram.cs
#if ANDROID
    builder.Services.AddSingleton<IPlatformService, AndroidPlatformService>();
#endif
```

---

## 📚 Platform-Specific Code Best Practices

### ✅ DO:

1. **Prefer MAUI's Cross-Platform APIs**
   - Use `FileSystem.AppDataDirectory` instead of platform paths
   - Use `MainThread.BeginInvokeOnMainThread()` instead of platform-specific threading
   - Use MAUI controls instead of native controls when possible

2. **Use Interfaces for Platform-Specific Features**
   - Create interfaces in your shared code
   - Implement in platform folders
   - Register in MauiProgram.cs

3. **Keep Platform Code Isolated**
   - Put platform code in Platforms folder
   - Don't scatter `#if` statements throughout your code
   - Use conditional compilation only in platform-specific files

4. **Document Platform Differences**
   - Comment why platform-specific code is needed
   - Note any platform-specific behaviors
   - Document any limitations

### ❌ DON'T:

1. **Don't Use Platform Checks for Business Logic**
   ```csharp
   // ❌ BAD
   #if ANDROID
       discount = 0.1;
   #elif IOS
       discount = 0.15;
   #endif
   ```

2. **Don't Duplicate MAUI Functionality**
   ```csharp
   // ❌ BAD - MAUI already provides this
   #if ANDROID
       var path = GetAndroidPath();
   #endif
   
   // ✅ GOOD
   var path = FileSystem.AppDataDirectory;
   ```

3. **Don't Forget to Test on All Platforms**
   - Platform-specific code needs testing on each platform
   - Use emulators/simulators for testing
   - Test on real devices when possible

---

## 🎯 Summary: Platform-Specific Code in 60 Seconds

**Why Platform-Specific Code?**
- Different platforms have different APIs, behaviors, and capabilities
- MAUI provides cross-platform APIs for common tasks
- Sometimes you need to access platform-specific features

**Two Main Approaches:**

1. **Conditional Compilation** (`#if ANDROID`)
   - Simple, quick
   - Good for small platform differences
   - Can clutter code if overused

2. **Interface Abstraction** (Better!)
   - Clean, maintainable
   - Testable
   - Professional approach
   - Use for significant platform features

**The Pattern:**
```
Interface → Platform Implementations → Register in MauiProgram → Inject in ViewModel
```

**Best Practices:**
- ✅ Use MAUI's cross-platform APIs when available
- ✅ Use interfaces for platform-specific features
- ✅ Keep platform code isolated in Platforms folder
- ❌ Don't use platform checks for business logic
- ❌ Don't duplicate MAUI functionality

---

## 🚀 Next Steps

Now that you understand platform-specific code, you're ready to:
- ✅ Access any platform-specific feature
- ✅ Build truly cross-platform apps
- ✅ Handle platform differences gracefully
- ✅ Write professional, maintainable code

**Next Lesson:** Local Storage - Learn how to store data locally on each platform using SQLite and MAUI's storage APIs!

---

## 🔧 Conditional Compilation

Use preprocessor directives to compile code for specific platforms.

### Platform Constants

```csharp
#if ANDROID
    // Android-specific code
#elif IOS
    // iOS-specific code
#elif WINDOWS
    // Windows-specific code
#elif MACCATALYST
    // macOS-specific code
#else
    // Fallback for other platforms
#endif
```

### Example: Platform-Specific File Path

```csharp
public class FileService
{
    public string GetDatabasePath()
    {
#if ANDROID
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "app.db");
#elif IOS
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", "app.db");
#elif WINDOWS
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "app.db");
#elif MACCATALYST
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "app.db");
#else
        throw new PlatformNotSupportedException();
#endif
    }
}
```

### Combining Conditions

```csharp
#if ANDROID || IOS
    // Mobile-specific code
#elif WINDOWS || MACCATALYST
    // Desktop-specific code
#endif
```

### Debug vs Release

```csharp
#if DEBUG
    Console.WriteLine("Debug mode - verbose logging");
#else
    Console.WriteLine("Release mode - minimal logging");
#endif
```

---

## 🎯 Abstraction with Interfaces

The best approach is to create interfaces and provide platform-specific implementations.

### Step 1: Define Interface

```csharp
// Services/IPlatformService.cs
public interface IPlatformService
{
    string GetPlatformName();
    string GetAppDataPath();
    void ShowToast(string message);
    void Vibrate(int durationMs);
}
```

### Step 2: Create Platform Implementations

**Android Implementation:**
```csharp
// Platforms/Android/AndroidPlatformService.cs
using Android.Widget;
using Android.OS;

[assembly: Dependency(typeof(AndroidPlatformService))]

namespace MyApp.Platforms.Android;

public class AndroidPlatformService : IPlatformService
{
    public string GetPlatformName() => "Android";
    
    public string GetAppDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
    
    public void ShowToast(string message)
    {
        var context = Android.App.Application.Context;
        Toast.MakeText(context, message, ToastLength.Short).Show();
    }
    
    public void Vibrate(int durationMs)
    {
        var vibrator = (Vibrator)Android.App.Application.Context
            .GetSystemService(Android.App.Context.VibratorService);
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
        {
            vibrator.Vibrate(VibrationEffect.CreateOneShot(durationMs, VibrationEffect.DefaultAmplitude));
        }
        else
        {
#pragma warning disable CS0618 // Type or member is obsolete
            vibrator.Vibrate(durationMs);
#pragma warning restore CS0618
        }
    }
}
```

**iOS Implementation:**
```csharp
// Platforms/iOS/iOSPlatformService.cs
using Foundation;
using UIKit;
using AudioToolbox;

[assembly: Dependency(typeof(iOSPlatformService))]

namespace MyApp.Platforms.iOS;

public class iOSPlatformService : IPlatformService
{
    public string GetPlatformName() => "iOS";
    
    public string GetAppDataPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
    }
    
    public void ShowToast(string message)
    {
        var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
        var okAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, _ => { });
        alert.AddAction(okAction);
        
        var controller = UIApplication.SharedApplication.KeyWindow.RootViewController;
        controller.PresentViewController(alert, true, null);
    }
    
    public void Vibrate(int durationMs)
    {
        SystemSound.Vibrate.PlaySystemSound();
    }
}
```

**Windows Implementation:**
```csharp
// Platforms/Windows/WindowsPlatformService.cs
using Windows.UI.Xaml;
using Windows.UI.Notifications;

[assembly: Dependency(typeof(WindowsPlatformService))]

namespace MyApp.Platforms.Windows;

public class WindowsPlatformService : IPlatformService
{
    public string GetPlatformName() => "Windows";
    
    public string GetAppDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
    
    public void ShowToast(string message)
    {
        var toastXml = $@"
            <toast>
                <visual>
                    <binding template='ToastGeneric'>
                        <text>{message}</text>
                    </binding>
                </visual>
            </toast>";
        
        var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
        xmlDoc.LoadXml(toastXml);
        
        var toast = new ToastNotification(xmlDoc);
        ToastNotificationManager.CreateToastNotifier().Show(toast);
    }
    
    public void Vibrate(int durationMs)
    {
        // Windows doesn't support vibration
        Console.WriteLine("Vibration not supported on Windows");
    }
}
```

### Step 3: Register in MauiProgram.cs

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts => { /* ... */ });
    
    // Register platform service
#if ANDROID
    builder.Services.AddSingleton<IPlatformService, AndroidPlatformService>();
#elif IOS
    builder.Services.AddSingleton<IPlatformService, iOSPlatformService>();
#elif WINDOWS
    builder.Services.AddSingleton<IPlatformService, WindowsPlatformService>();
#elif MACCATALYST
    builder.Services.AddSingleton<IPlatformService, MacPlatformService>();
#endif
    
    return builder.Build();
}
```

### Step 4: Use in ViewModel

```csharp
public class MyViewModel : ObservableObject
{
    private readonly IPlatformService _platform;
    
    public MyViewModel(IPlatformService platform)
    {
        _platform = platform;
    }
    
    [RelayCommand]
    private void ShowNotification()
    {
        _platform.ShowToast("Hello from " + _platform.GetPlatformName());
    }
    
    [RelayCommand]
    private void Vibrate()
    {
        _platform.Vibrate(500);
    }
}
```

---

## 🎨 Common Platform-Specific Features

### Biometric Authentication

```csharp
// Services/IBiometricService.cs
public interface IBiometricService
{
    Task<bool> AuthenticateAsync(string reason);
    bool IsAvailable();
}

// Platforms/Android/BiometricService.cs
using Android.Hardware.Biometrics;
using AndroidX.Fragment.App;

public class BiometricService : IBiometricService
{
    public bool IsAvailable()
    {
        var context = Android.App.Application.Context;
        return AndroidX.Core.Hardware.Fingerprint.FingerprintManagerCompat.From(context)
            .IsHardwareDetected;
    }
    
    public async Task<bool> AuthenticateAsync(string reason)
    {
        var activity = Platform.CurrentActivity;
        var promptInfo = new PromptInfo.Builder()
            .SetTitle("Authentication")
            .SetDescription(reason)
            .SetNegativeButtonText("Cancel")
            .Build();
        
        var callback = new BiometricCallback();
        var biometricPrompt = new BiometricPrompt(
            (FragmentActivity)activity,
            Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S
                ? new AndroidX.Core.OS.CancellationSignal()
                : null,
            callback);
        
        biometricPrompt.Authenticate(promptInfo);
        
        return await callback.Task;
    }
    
    private class BiometricCallback : BiometricPrompt.IAuthenticationCallback
    {
        private TaskCompletionSource<bool> _tcs = new();
        
        public Task<bool> Task => _tcs.Task;
        
        public void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
        {
            _tcs.SetResult(true);
        }
        
        public void OnAuthenticationFailed()
        {
            _tcs.SetResult(false);
        }
        
        public void OnAuthenticationError(int errorCode, ICharSequence errString)
        {
            _tcs.SetResult(false);
        }
    }
}
```

### File Picker

```csharp
// Services/IFilePickerService.cs
public interface IFilePickerService
{
    Task<string> PickFileAsync();
    Task<string> SaveFileAsync(string defaultName);
}

// Platforms/Windows/WindowsFilePickerService.cs
using Windows.Storage;
using Windows.Storage.Pickers;

public class WindowsFilePickerService : IFilePickerService
{
    public async Task<string> PickFileAsync()
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add("*");
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
    
    public async Task<string> SaveFileAsync(string defaultName)
    {
        var picker = new FileSavePicker();
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("All Files", new List<string> { "." });
        picker.SuggestedFileName = defaultName;
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        
        var file = await picker.PickSaveFileAsync();
        return file?.Path;
    }
}
```

### Battery Information

```csharp
// Services/IBatteryService.cs
public interface IBatteryService
{
    double GetChargeLevel();
    BatteryState GetState();
    PowerSource GetPowerSource();
}

// Platforms/Android/AndroidBatteryService.cs
using Android.Content;
using Android.OS;

public class AndroidBatteryService : IBatteryService
{
    public double GetChargeLevel()
    {
        using var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Android.App.Application.Context.RegisterReceiver(null, filter);
        
        int level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
        int scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);
        
        return (double)level / scale;
    }
    
    public BatteryState GetState()
    {
        using var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Android.App.Application.Context.RegisterReceiver(null, filter);
        
        var status = battery.GetIntExtra(BatteryManager.ExtraStatus, -1);
        
        return status switch
        {
            (int)BatteryStatus.Charging => BatteryState.Charging,
            (int)BatteryStatus.Discharging => BatteryState.Discharging,
            (int)BatteryStatus.Full => BatteryState.Full,
            (int)BatteryStatus.NotCharging => BatteryState.NotCharging,
            _ => BatteryState.Unknown
        };
    }
    
    public PowerSource GetPowerSource()
    {
        using var filter = new IntentFilter(Intent.ActionBatteryChanged);
        var battery = Android.App.Application.Context.RegisterReceiver(null, filter);
        
        var chargePlug = battery.GetIntExtra(BatteryManager.ExtraPlugged, -1);
        
        return chargePlug switch
        {
            (int)BatteryPlugged.Usb => PowerSource.Usb,
            (int)BatteryPlugged.Ac => PowerSource.Ac,
            (int)BatteryPlugged.Wireless => PowerSource.Wireless,
            _ => PowerSource.Battery
        };
    }
}
```

---

## 🎯 Complete Example: Cross-Platform Logger

Let's build a logger that works differently on each platform.

### Interface

```csharp
// Services/ILogger.cs
public interface ILogger
{
    void Log(string message);
    void LogError(string error);
    void LogWarning(string warning);
}
```

### Implementations

**Android (Logcat):**
```csharp
// Platforms/Android/AndroidLogger.cs
using Android.Util;

public class AndroidLogger : ILogger
{
    public void Log(string message)
    {
        Log.Debug("MyApp", message);
    }
    
    public void LogError(string error)
    {
        Log.Error("MyApp", error);
    }
    
    public void LogWarning(string warning)
    {
        Log.Warn("MyApp", warning);
    }
}
```

**iOS (Console):**
```csharp
// Platforms/iOS/iOSLogger.cs
using Foundation;

public class iOSLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }
    
    public void LogError(string error)
    {
        Console.WriteLine($"[ERROR] {error}");
    }
    
    public void LogWarning(string warning)
    {
        Console.WriteLine($"[WARN] {warning}");
    }
}
```

**Windows (Event Log):**
```csharp
// Platforms/Windows/WindowsLogger.cs
using System.Diagnostics;

public class WindowsLogger : ILogger
{
    public void Log(string message)
    {
        Debug.WriteLine($"[INFO] {message}");
    }
    
    public void LogError(string error)
    {
        Debug.WriteLine($"[ERROR] {error}");
    }
    
    public void LogWarning(string warning)
    {
        Debug.WriteLine($"[WARN] {warning}");
    }
}
```

### Registration

```csharp
// MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts => { /* ... */ });
    
#if ANDROID
    builder.Services.AddSingleton<ILogger, AndroidLogger>();
#elif IOS
    builder.Services.AddSingleton<ILogger, iOSLogger>();
#elif WINDOWS
    builder.Services.AddSingleton<ILogger, WindowsLogger>();
#elif MACCATALYST
    builder.Services.AddSingleton<ILogger, MacLogger>();
#endif
    
    return builder.Build();
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Platform-Specific Theme

Create a theme service that adapts to platform:

**Interface:**
```csharp
public interface IThemeService
{
    Color GetPrimaryColor();
    Color GetSecondaryColor();
    string GetFontFamily();
    double GetButtonHeight();
}
```

**Requirements:**
- Android: Material Design colors, Roboto font, 48dp height
- iOS: SF Symbols, SF Pro font, 44pt height
- Windows: Fluent colors, Segoe UI, 32px height
- Register in DI
- Use in multiple pages

### Exercise 2: File System Service

Create a file service with platform-specific paths:

**Interface:**
```csharp
public interface IFileSystemService
{
    string GetDocumentsPath();
    string GetCachePath();
    string GetTempPath();
    bool FileExists(string path);
    void WriteText(string path, string content);
    string ReadText(string path);
}
```

**Requirements:**
- Use correct paths for each platform
- Handle platform-specific permissions
- Create directories if needed
- Handle exceptions gracefully
- Write unit tests (mock the interface)

### Exercise 3: Network Connectivity

Create a network service that checks connectivity:

**Interface:**
```csharp
public interface INetworkService
{
    bool IsConnected { get; }
    NetworkType GetNetworkType();
    event EventHandler<NetworkStatusEventArgs> ConnectivityChanged;
}

public enum NetworkType
{
    None,
    WiFi,
    Cellular,
    Ethernet,
    Unknown
}
```

**Requirements:**
- Use platform-specific APIs (ConnectivityManager on Android, Reachability on iOS)
- Monitor connectivity changes
- Raise events when status changes
- Display connectivity status in UI
- Show warning when offline

---

## ❓ Common Beginner Questions

### Q: Should I use conditional compilation or interfaces?

**A**: 
- **Interfaces**: Preferred for reusable services
- **Conditional compilation**: For small, one-off platform differences

### Q: How do I test platform-specific code?

**A**: Use interfaces and mock them in tests. Never test platform-specific implementations directly.

### Q: Can I access platform APIs without interfaces?

**A**: Yes, using `Platform.CurrentActivity` (Android) or `UIApplication.SharedApplication` (iOS), but it's not recommended.

### Q: What if a feature isn't supported on a platform?

**A**: Either:
1. Throw `PlatformNotSupportedException`
2. Implement fallback behavior
3. Use conditional compilation to exclude entirely

### Q: How do I handle platform-specific permissions?

**A**: Each platform has its own system:
- Android: AndroidManifest.xml
- iOS: Info.plist
- Windows: Package.appxmanifest
- macOS: Entitlements.plist

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **Conditional Compilation** | Compile code for specific platforms |
| **Interface Abstraction** | Define contracts, implement per platform |
| **Dependency Injection** | Inject platform-specific implementations |
| **Platform APIs** | Use platform SDKs when needed |
| **Fallback Behavior** | Handle unsupported features gracefully |

---

## ✅ Checklist

Before moving to Lesson 8, make sure you can:

- [ ] Use conditional compilation for platform-specific code
- [ ] Create interfaces for platform services
- [ ] Implement platform-specific versions
- [ ] Register services in MauiProgram.cs
- [ ] Inject and use platform services
- [ ] Handle platform differences gracefully
- [ ] Access platform-specific APIs

---

## 🚀 Next Steps

Excellent! You now understand how to handle platform-specific code. In the next lesson, we'll learn about **Local Storage** and how to persist data in your MAUI app.

**Next Lesson**: [Lesson 8: Local Storage](08_Local_Storage.md)

---

## 📖 Additional Reading

- [Platform Integration](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/)
- [Platform-Specific APIs](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/platform-specifics/)
- [Dependency Service](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/dependency-service/)
- [Conditional Compilation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives/)
