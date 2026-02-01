# Lesson 10: Deployment

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand the deployment process for each platform
- Prepare your app for release
- Build release versions for Android, iOS, Windows, and macOS
- Sign and publish to app stores
- Handle app updates and versioning
- Troubleshoot common deployment issues

---

## 📖 Deployment Overview

Deployment is the process of building and distributing your app to users. Each platform has its own requirements and processes.

### Platform Deployment Comparison

| Platform | Build Tool | Store | Review Time | Cost |
|----------|-----------|-------|-------------|------|
| **Android** | Visual Studio / CLI | Google Play Store | 1-3 days | $25 (one-time) |
| **iOS** | Visual Studio / Mac | App Store | 1-7 days | $99/year |
| **Windows** | Visual Studio | Microsoft Store | 1-3 days | Free (optional) |
| **macOS** | Visual Studio / Mac | Mac App Store | 1-7 days | $99/year |

---

## 🛠️ Pre-Deployment Checklist

Before building for release, ensure your app is ready.

### 1. Update App Information

**Android (AndroidManifest.xml):**
```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application
        android:label="My App"
        android:icon="@mipmap/appicon">
        
        <!-- Permissions -->
        <uses-permission android:name="android.permission.INTERNET" />
        <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
        
    </application>
</manifest>
```

**iOS (Info.plist):**
```xml
<key>CFBundleDisplayName</key>
<string>My App</string>
<key>CFBundleVersion</key>
<string>1.0.0</string>
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSAllowsArbitraryLoads</key>
    <true/>
</dict>
```

**Windows (Package.appxmanifest):**
```xml
<Properties>
    <DisplayName>My App</DisplayName>
    <PublisherDisplayName>Your Company</PublisherDisplayName>
    <Version>1.0.0.0</Version>
</Properties>
```

### 2. Update Version Numbers

**In your .csproj file:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net8.0-android</TargetFramework>
        <!-- Add version info -->
        <ApplicationVersion>1</ApplicationVersion>
        <ApplicationDisplayVersion>1.0.0</ApplicationDisplayVersion>
    </PropertyGroup>
</Project>
```

**Or programmatically:**
```csharp
// In MauiProgram.cs or App.xaml.cs
public static string AppVersion => 
    typeof(App).Assembly.GetName().Version?.ToString() ?? "1.0.0.0";
```

### 3. Remove Debug Code

```csharp
#if DEBUG
    // Debug-only code
    builder.Logging.AddDebug();
#endif
```

### 4. Optimize Resources

- Remove unused images
- Compress images
- Use appropriate image sizes
- Remove test data

### 5. Test on Real Devices

Always test on physical devices before release!

---

## 📱 Android Deployment

### Step 1: Create Keystore (Signing Key)

The keystore is used to sign your app uniquely.

**Using command line:**
```bash
keytool -genkey -v -keystore myapp-release.keystore -alias myapp-keyalias -keyalg RSA -keysize 2048 -validity 10000
```

**Or using Android Studio:**
1. Build → Generate Signed Bundle/APK
2. Create new keystore
3. Fill in details

### Step 2: Configure Signing in .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net8.0-android</TargetFramework>
        
        <!-- Signing configuration -->
        <AndroidKeyStore>True</AndroidKeyStore>
        <AndroidSigningKeyStore>myapp-release.keystore</AndroidSigningKeyStore>
        <AndroidSigningKeyAlias>myapp-keyalias</AndroidSigningKeyAlias>
        <AndroidSigningKeyPass>your_keystore_password</AndroidSigningKeyPass>
        <AndroidSigningStorePass>your_store_password</AndroidSigningStorePass>
    </PropertyGroup>
</Project>
```

**⚠️ SECURITY WARNING**: Never commit passwords to source control!

**Better approach - use environment variables:**
```xml
<AndroidSigningKeyPass>$(AndroidSigningKeyPass)</AndroidSigningKeyPass>
<AndroidSigningStorePass>$(AndroidSigningStorePass)</AndroidSigningStorePass>
```

### Step 3: Build Release APK

**Using Visual Studio:**
1. Select "Release" configuration
2. Target "Android" platform
3. Build → Build Solution
4. Find APK in: `bin/Release/net8.0-android/publish/`

**Using command line:**
```bash
dotnet publish -c Release -f net8.0-android
```

### Step 4: Build App Bundle (AAB) for Play Store

Google Play requires App Bundle format.

```bash
dotnet publish -c Release -f net8.0-android /p:AndroidPackageFormat=aab
```

### Step 5: Upload to Google Play Console

1. Go to [Google Play Console](https://play.google.com/console)
2. Create new app (if not exists)
3. Complete store listing:
   - Title, description, screenshots
   - Icon (512x512 PNG)
   - Feature graphic (1024x500 PNG)
4. Upload AAB file
5. Set pricing and distribution
6. Submit for review

---

## 🍎 iOS Deployment

### Requirements

- **Mac computer** (or Mac build cloud service)
- **Xcode** (from App Store)
- **Apple Developer Account** ($99/year)
- **Visual Studio for Mac** or **Visual Studio 2022 with Mac agent**

### Step 1: Configure Provisioning Profiles

1. Go to [Apple Developer Portal](https://developer.apple.com/)
2. Create App ID
3. Create Distribution Certificate
4. Create Provisioning Profile
5. Download and install on your Mac

### Step 2: Configure Info.plist

```xml
<key>CFBundleIdentifier</key>
<string>com.yourcompany.myapp</string>
<key>CFBundleVersion</key>
<string>1.0.0</string>
<key>CFBundleShortVersionString</key>
<string>1.0</string>
<key>UIRequiredDeviceCapabilities</key>
<array>
    <string>arm64</string>
</array>
<key>UISupportedInterfaceOrientations</key>
<array>
    <string>UIInterfaceOrientationPortrait</string>
</array>
```

### Step 3: Build for iOS

**Using Visual Studio (with Mac build agent):**
1. Select "Release" configuration
2. Target "iOS" platform
3. Select device "Remote Device" (your Mac)
4. Build → Build Solution

**Using command line (on Mac):**
```bash
dotnet build -c Release -f net8.0-ios
```

### Step 4: Archive and Publish

**Using Xcode:**
1. Open generated `.sln` or `.csproj` in Xcode
2. Product → Archive
3. Window → Organizer
4. Select archive → Distribute App
5. Choose distribution method:
   - **App Store Connect** (for public release)
   - **Ad Hoc** (for testing)
   - **Enterprise** (for internal distribution)
6. Upload to App Store Connect

### Step 5: Submit to App Store

1. Go to [App Store Connect](https://appstoreconnect.apple.com/)
2. Create new app (if not exists)
3. Complete app information:
   - Name, description, keywords
   - Screenshots (required for all device sizes)
   - App icon (1024x1024 PNG)
4. Upload build from Xcode
5. Submit for review

---

## 🪟 Windows Deployment

### Step 1: Configure Package.appxmanifest

```xml
<Package>
    <Identity Name="MyApp" 
              Publisher="CN=YourCompany" 
              Version="1.0.0.0" />
    
    <Properties>
        <DisplayName>My App</DisplayName>
        <PublisherDisplayName>Your Company</PublisherDisplayName>
        <Logo>Assets\StoreLogo.png</Logo>
    </Properties>
    
    <Applications>
        <Application Id="App" 
                    Executable="$targetnametoken$.exe" 
                    EntryPoint="$targetentrypoint$">
            <uap:VisualElements 
                DisplayName="My App"
                Square150x150Logo="Assets\Square150x150Logo.png"
                Square44x44Logo="Assets\Square44x44Logo.png"
                Description="My awesome app"
                BackgroundColor="#464646">
            </uap:VisualElements>
        </Application>
    </Applications>
</Package>
```

### Step 2: Build Release Version

**Using Visual Studio:**
1. Select "Release" configuration
2. Target "windows-x64" platform
3. Build → Build Solution
4. Find MSIX in: `bin/Release/net8.0-windows10.0.19041.0/win10-x64/`

**Using command line:**
```bash
dotnet publish -c Release -f net8.0-windows10.0.19041.0 -r win-x64 --self-contained
```

### Step 3: Test MSIX Package

Double-click the `.msix` file to install locally.

### Step 4: Upload to Microsoft Store

1. Go to [Partner Center](https://partner.microsoft.com/dashboard)
2. Create new app (if not exists)
3. Complete app submission:
   - App name, description, category
   - Screenshots
   - App icons
4. Upload MSIX package
5. Submit for certification

### Alternative: Sideloading

For enterprise distribution without store:

```bash
# Create self-signed certificate
makecert -n "CN=YourCompany" -r -sv MyApp.pvk MyApp.cer
pvk2pfx -pvk MyApp.pvk -spc MyApp.cer -pfx MyApp.pfx

# Sign the package
signtool sign -f MyApp.pfx -fd SHA256 -v MyApp.msix
```

---

## 🖥️ macOS Deployment

### Step 1: Configure Entitlements.plist

```xml
<key>com.apple.security.app-sandbox</key>
<true/>
<key>com.apple.security.files.user-selected.read-write</key>
<true/>
<key>com.apple.security.network.client</key>
<true/>
```

### Step 2: Build for macOS

**Using Visual Studio for Mac:**
1. Select "Release" configuration
2. Target "maccatalyst" platform
3. Build → Build Solution

**Using command line (on Mac):**
```bash
dotnet build -c Release -f net8.0-maccatalyst
```

### Step 3: Create App Package

```bash
dotnet publish -c Release -f net8.0-maccatalyst -r osx-x64
```

### Step 4: Notarize for Distribution

macOS requires notarization for distribution outside App Store.

```bash
# Create app package
dotnet publish -c Release -f net8.0-maccatalyst -r osx-x64 -p:PublishSingleFile=false

# Sign with developer certificate
codesign --deep --force --verify --verbose --sign "Developer ID Application: Your Name" MyApp.app

# Notarize
xcrun notarytool submit MyApp.app.zip --apple-id "your@email.com" --password "app-specific-password" --team-id "TEAMID" --wait

# Staple notary ticket
xcrun stapler staple MyApp.app
```

### Step 5: Submit to Mac App Store

Similar to iOS process, using App Store Connect.

---

## 🔄 Versioning and Updates

### Semantic Versioning

Use semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking changes
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, backward compatible

**Examples:**
- `1.0.0` → `1.0.1` (bug fix)
- `1.0.0` → `1.1.0` (new feature)
- `1.0.0` → `2.0.0` (breaking changes)

### Update Strategy

```csharp
// Services/IUpdateService.cs
public interface IUpdateService
{
    Task CheckForUpdatesAsync();
    Task<string> GetCurrentVersionAsync();
}

// Services/UpdateService.cs
public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private const string UpdateCheckUrl = "https://api.example.com/version";
    
    public async Task<string> GetCurrentVersionAsync()
    {
        return AppInfo.VersionString;
    }
    
    public async Task CheckForUpdatesAsync()
    {
        var currentVersion = await GetCurrentVersionAsync();
        var latestVersion = await GetLatestVersionAsync();
        
        if (IsNewerVersion(latestVersion, currentVersion))
        {
            await ShowUpdateDialogAsync(latestVersion);
        }
    }
    
    private bool IsNewerVersion(string latest, string current)
    {
        var latestParts = latest.Split('.').Select(int.Parse).ToArray();
        var currentParts = current.Split('.').Select(int.Parse).ToArray();
        
        for (int i = 0; i < 3; i++)
        {
            if (latestParts[i] > currentParts[i])
                return true;
            if (latestParts[i] < currentParts[i])
                return false;
        }
        return false;
    }
}
```

---

## 🧪 Testing Before Deployment

### Pre-Release Testing Checklist

- [ ] Test on all target platforms
- [ ] Test on multiple device sizes
- [ ] Test with poor network conditions
- [ ] Test with airplane mode
- [ ] Test with low battery
- [ ] Test app lifecycle (background/foreground)
- [ ] Test permissions requests
- [ ] Test deep links
- [ ] Test notifications
- [ ] Performance testing
- [ ] Security testing
- [ ] Accessibility testing

### Beta Testing

**Android:**
- Google Play Console Internal Test
- Google Play Console Closed Track
- Google Play Console Open Track

**iOS:**
- TestFlight (up to 10,000 testers)
- Ad Hoc distribution (100 devices)

**Windows:**
- Microsoft Store beta testing
- Sideloading for internal testing

---

## ❓ Common Deployment Issues

### Issue: "INSTALL_FAILED_UPDATE_INCOMPATIBLE" on Android

**Solution**: Increment version number or uninstall old version first.

### Issue: "Code signing failed" on iOS

**Solution**: 
1. Verify provisioning profile matches bundle ID
2. Check certificate is valid
3. Clean and rebuild

### Issue: "App won't install on Windows"

**Solution**:
1. Check Windows version requirement
2. Verify signing certificate
3. Check capabilities in manifest

### Issue: "App rejected from App Store"

**Common reasons**:
- Incomplete metadata
- Missing screenshots
- Violates guidelines
- Bugs found during review
- In-app purchase issues

**Solution**: Read rejection message carefully, fix issues, resubmit.

### Issue: "App crashes on release build but works in debug"

**Possible causes**:
- Debug-only code not wrapped in `#if DEBUG`
- Different optimization settings
- Missing resources
- Code linking issues

**Solution**: 
1. Check for debug-only code
2. Test release build early
3. Use linker settings carefully

---

## 📊 Post-Deployment Monitoring

### Analytics

Implement analytics to track app usage:

```csharp
// Services/IAnalyticsService.cs
public interface IAnalyticsService
{
    void TrackEvent(string eventName, Dictionary<string, string> properties = null);
    void TrackError(Exception exception, Dictionary<string, string> properties = null);
}

// Use App Center, Firebase, or similar
```

### Crash Reporting

```csharp
// In MauiProgram.cs
#if DEBUG
    builder.Logging.AddDebug();
#else
    // Add crash reporting service
    builder.Services.AddSingleton<ICrashReportingService, AppCenterCrashService>();
#endif
```

### Feedback Mechanism

```csharp
public class FeedbackViewModel
{
    [RelayCommand]
    private async Task SendFeedbackAsync()
    {
        var subject = $"Feedback - {AppInfo.Name} v{AppInfo.VersionString}";
        var body = FeedbackText;
        
        await Launcher.OpenAsync(new Uri(
            $"mailto:support@example.com?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}"));
    }
}
```

---

## 🎯 Complete Deployment Workflow

### 1. Pre-Release (1-2 weeks before)

- [ ] Complete all features
- [ ] Fix all known bugs
- [ ] Test on all platforms
- [ ] Prepare store assets (icons, screenshots)
- [ ] Write store descriptions
- [ ] Create developer accounts
- [ ] Set up analytics

### 2. Release Day

- [ ] Update version numbers
- [ ] Build release versions
- [ ] Test release builds
- [ ] Upload to stores
- [ ] Submit for review
- [ ] Monitor review status

### 3. Post-Release

- [ ] Monitor crash reports
- [ ] Respond to reviews
- [ ] Fix critical bugs
- [ ] Plan next version
- [ ] Gather user feedback

---

## 🧪 Practice Exercises

### Exercise 1: Prepare for Android Release

Take your existing app and prepare for Android release:

**Requirements:**
- Create keystore
- Configure signing in .csproj
- Build release APK
- Build release AAB
- Test on physical device
- Prepare Play Store listing

### Exercise 2: Prepare for iOS Release

Prepare your app for iOS (requires Mac):

**Requirements:**
- Create App ID in Developer Portal
- Create provisioning profile
- Configure Info.plist
- Build and archive in Xcode
- Prepare App Store Connect listing

### Exercise 3: Multi-Platform Release

Prepare your app for all platforms:

**Requirements:**
- Create release builds for Android, iOS, Windows
- Test on all platforms
- Prepare store listings for each
- Document version changes
- Create release notes

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **Keystore** | Android signing key |
| **Provisioning Profile** | iOS signing certificate |
| **AAB** | Android App Bundle (Play Store) |
| **APK** | Android Package (sideloading) |
| **MSIX** | Windows package format |
| **Versioning** | MAJOR.MINOR.PATCH format |
| **Beta Testing** | Pre-release testing |

---

## ✅ Final Checklist

Before completing this course, make sure you can:

- [ ] Build release versions for Android, iOS, Windows, macOS
- [ ] Sign apps with appropriate certificates
- [ ] Upload to app stores
- [ ] Handle versioning and updates
- [ ] Troubleshoot deployment issues
- [ ] Implement analytics and crash reporting
- [ ] Prepare store listings and assets

---

## 🎉 Congratulations!

You've completed the **.NET MAUI Basics** course! You now have the skills to:

✅ Create cross-platform apps with MAUI  
✅ Build beautiful UIs with XAML  
✅ Implement MVVM architecture  
✅ Use data binding effectively  
✅ Navigate between pages  
✅ Manage dependencies with DI  
✅ Handle platform-specific code  
✅ Store data locally  
✅ Integrate with APIs  
✅ Deploy to app stores  

### What's Next?

Continue your MAUI journey with:

1. **Advanced Topics**
   - Custom renderers and effects
   - Animations and transitions
   - Background tasks
   - Push notifications

2. **Specialized Libraries**
   - MAUI Community Toolkit
   - Syncfusion MAUI controls
   - Telerik UI for MAUI
   - DevExpress MAUI

3. **Real-World Projects**
   - Build a complete app
   - Contribute to open source
   - Join MAUI community
   - Share your knowledge

### Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [MAUI Community Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/)
- [MAUI Samples](https://github.com/dotnet/maui-samples)
- [MAUI GitHub](https://github.com/dotnet/maui)
- [.NET Blog](https://devblogs.microsoft.com/dotnet/)

---

**Happy coding! 🚀**
