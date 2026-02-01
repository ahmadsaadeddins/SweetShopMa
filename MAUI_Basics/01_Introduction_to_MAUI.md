# Lesson 1: Introduction to .NET MAUI

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand what .NET MAUI is and why it's important
- Set up your development environment
- Create your first MAUI application
- Understand the basic project structure
- Run your app on different platforms

---

## 📖 What is .NET MAUI?

**.NET MAUI** (Multi-platform App UI) is Microsoft's cross-platform framework for building native mobile and desktop applications. It's the evolution of Xamarin.Forms, designed to make cross-platform development simpler and more efficient.

### Why Use .NET MAUI?

✅ **Single Codebase** - Write once, run on Android, iOS, Windows, and macOS  
✅ **Native Performance** - Apps use native platform controls, not web views  
✅ **Modern Tooling** - Built on .NET 6+ with latest C# features  
✅ **Hot Reload** - See changes instantly without rebuilding  
✅ **Strong Ecosystem** - Access to NuGet packages and .NET libraries  
✅ **Free & Open Source** - No licensing fees, community-driven

### What Can You Build?

- 🛒 **E-commerce Apps** - Online stores with payment integration
- 📱 **Social Apps** - Chat, social media, sharing features
- 📊 **Business Apps** - Inventory, CRM, productivity tools
- 🎮 **Games** - Casual games with 2D graphics
- 📚 **Educational Apps** - Learning platforms, quizzes
- 🏥 **Health & Fitness** - Trackers, workout apps

---

## 🛠️ Setting Up Your Development Environment

### Step 1: Install Visual Studio 2022

1. Download Visual Studio 2022 Community (free) from [visualstudio.microsoft.com](https://visualstudio.microsoft.com/)
2. Run the installer
3. **CRITICAL**: In the workload selection, check **".NET Multi-platform App UI development"**
4. Click "Install" (this may take 30-60 minutes)

### What Gets Installed?

The MAUI workload includes:
- **.NET SDK** (latest version)
- **Android SDK** + Emulators
- **Windows App SDK** (for Windows apps)
- **MAUI templates** for project creation
- **Xamarin tools** for debugging

### Step 2: Verify Installation

1. Open Visual Studio 2022
2. Click "Create a new project"
3. Search for "MAUI" in the templates
4. You should see ".NET MAUI App" template

### Step 3: (Optional) Mac Setup for iOS Development

To build for iOS, you need:
- A Mac computer
- Xcode from the App Store
- Visual Studio for Mac (or pair with Windows)

**Note**: You can develop and test on Windows without a Mac. You'll just need a Mac (or Mac build cloud service) to create the final iOS app.

---

## 🚀 Creating Your First MAUI App

### Step 1: Create the Project

1. In Visual Studio, click **"Create a new project"**
2. Search for **"MAUI"** and select **".NET MAUI App"**
3. Name your project: **"MyFirstMauiApp"**
4. Choose a location and click **"Create"**

### Step 2: Explore the Project Structure

Visual Studio creates several files and folders. Let's understand what each does:

```
MyFirstMauiApp/
├── MauiProgram.cs          # ⭐ App configuration & startup
├── App.xaml                # App-level resources
├── App.xaml.cs             # App entry point
├── AppShell.xaml           # Navigation structure
├── AppShell.xaml.cs        # Navigation logic
├── MainPage.xaml           # ⭐ First page UI (XAML)
├── MainPage.xaml.cs        # First page code-behind
├── Resources/              # Images, fonts, styles, strings
│   ├── Images/
│   ├── Fonts/
│   ├── Styles/
│   └── Raw/
├── Platforms/              # Platform-specific code
│   ├── Android/
│   ├── iOS/
│   ├── Windows/
│   └── MacCatalyst/
└── MyFirstMauiApp.csproj   # Project configuration
```

### Key Files Explained

#### **MauiProgram.cs** - The Heart of Your App

This is where your app starts. It configures services, fonts, and dependency injection.

```csharp
using Microsoft.Extensions.Logging;

namespace MyFirstMauiApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()                    // Set the main App class
            .ConfigureFonts(fonts =>              // Register custom fonts
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

**What's happening here?**
1. `MauiApp.CreateBuilder()` - Creates a builder to configure your app
2. `UseMauiApp<App>()` - Tells MAUI which class is the main App
3. `ConfigureFonts()` - Registers fonts for use in XAML
4. `builder.Build()` - Creates the final app instance

#### **MainPage.xaml** - Your First UI

XAML (pronounced "zamel") is a markup language for defining UI:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyFirstMauiApp.MainPage">

    <ScrollView>
        <VerticalStackLayout 
            Spacing="25" 
            Padding="30,0" 
            VerticalOptions="Center">

            <Label 
                Text="Hello, World!"
                SemanticProperties.HeadingLevel="Level1"
                FontSize="32"
                HorizontalOptions="Center" />

            <Label 
                Text="Welcome to .NET MAUI!"
                SemanticProperties.HeadingLevel="Level2"
                SemanticProperties.Description="Welcome to .NET Multi-platform App UI"
                FontSize="18"
                HorizontalOptions="Center" />

            <Button 
                x:Name="CounterBtn"
                Text="Click me"
                Clicked="OnCounterClicked"
                HorizontalOptions="Center" />

        </VerticalStackLayout>
    </ScrollView>

</ContentPage>
```

**Understanding the XAML:**
- `<ContentPage>` - The main container for a page
- `<ScrollView>` - Allows content to scroll if it's too large
- `<VerticalStackLayout>` - Arranges children vertically
- `<Label>` - Displays text
- `<Button>` - A clickable button
- Properties like `Text`, `FontSize`, `HorizontalOptions` control appearance

#### **MainPage.xaml.cs** - Code-Behind Logic

This file contains C# code that responds to user interactions:

```csharp
namespace MyFirstMauiApp;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
```

**What's happening:**
1. `InitializeComponent()` - Loads the XAML UI (auto-generated)
2. `OnCounterClicked()` - Runs when button is clicked
3. Updates button text with click count
4. `SemanticScreenReader.Announce()` - Helps screen readers announce changes

---

## 🏃 Running Your First App

### Option 1: Windows (Recommended for Beginners)

1. In Visual Studio toolbar, select **"Windows Machine"** from the device dropdown
2. Click the green **Play** button (or press F5)
3. Your app will launch as a Windows desktop app!

### Option 2: Android Emulator

1. Start the Android Emulator:
   - Click device dropdown → "Android Emulator Manager"
   - Create a new emulator (e.g., Pixel 5)
   - Start the emulator
2. In Visual Studio, select your emulator from device dropdown
3. Click Play (F5)

### Option 3: Hot Reload (Magic!)

**Hot Reload** lets you see UI changes instantly without restarting:

1. Run your app (Windows or Android)
2. While app is running, change something in MainPage.xaml:
   ```xml
   <Label Text="Hello, World!" 
          FontSize="32" 
          TextColor="Blue" />  <!-- Added this -->
   ```
3. Press **Ctrl+S** to save
4. Watch the app update instantly!

**This is a huge time-saver!** Use it frequently while designing UIs.

---

## 🎨 Customizing Your First App

Let's make the app more interesting:

### Exercise 1: Change the Colors

Update MainPage.xaml:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyFirstMauiApp.MainPage"
             BackgroundColor="#F0F0F0">  <!-- Light gray background -->

    <ScrollView>
        <VerticalStackLayout Spacing="25" Padding="30,0" VerticalOptions="Center">

            <Label Text="👋 Hello, World!"
                   FontSize="32"
                   FontAttributes="Bold"
                   TextColor="#2E7D32"  <!-- Green -->
                   HorizontalOptions="Center" />

            <Label Text="Welcome to .NET MAUI!"
                   FontSize="18"
                   TextColor="#424242"  <!-- Dark gray -->
                   HorizontalOptions="Center" />

            <Button x:Name="CounterBtn"
                    Text="Click me"
                    Clicked="OnCounterClicked"
                    BackgroundColor="#1976D2"  <!-- Blue -->
                    TextColor="White"
                    WidthRequest="200"
                    HeightRequest="50" />

        </VerticalStackLayout>
    </ScrollView>

</ContentPage>
```

### Exercise 2: Add an Image

1. Add an image to `Resources/Images/` (e.g., `logo.png`)
2. Update MainPage.xaml:

```xml
<Image Source="logo.png"
       WidthRequest="150"
       HeightRequest="150"
       HorizontalOptions="Center" />
```

### Exercise 3: Add an Entry Field

```xml
<Entry x:Name="NameEntry"
       Placeholder="Enter your name"
       HorizontalOptions="Center"
       WidthRequest="300" />

<Label x:Name="GreetingLabel"
       Text="Hello, stranger!"
       FontSize="18"
       HorizontalOptions="Center" />
```

Update MainPage.xaml.cs:

```csharp
private void OnCounterClicked(object sender, EventArgs e)
{
    string name = NameEntry.Text;
    
    if (!string.IsNullOrWhiteSpace(name))
        GreetingLabel.Text = $"Hello, {name}!";
    else
        GreetingLabel.Text = "Hello, stranger!";
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Temperature Converter

Create a simple temperature converter:

1. Add an Entry for Celsius input
2. Add a Button to convert
3. Add a Label to show Fahrenheit result
4. Formula: `F = C × 9/5 + 32`

**Hint**: You'll need to parse the Entry text: `double.Parse(Entry.Text)`

### Exercise 2: Color Picker

Create a color picker app:

1. Add three Sliders (Red, Green, Blue) - range 0-255
2. Add a BoxView to show the color
3. Update the BoxView color as sliders change
4. Display the RGB values

**Hint**: Use `Color.FromRgb(r, g, b)` where values are 0-255

### Exercise 3: Counter App

Build a full counter app:

1. Add "+" and "-" buttons
2. Display current count
3. Add a "Reset" button
4. Change color based on count (positive = green, negative = red, zero = gray)

---

## ❓ Common Beginner Questions

### Q: Why does my app show a blank screen?

**A**: Make sure you've set the correct device in the dropdown. If using Android, ensure the emulator is running first.

### Q: What's the difference between XAML and C# for UI?

**A**: XAML is declarative (describes WHAT the UI looks like), C# is imperative (describes HOW it behaves). Use XAML for layout, C# for logic.

### Q: Can I use C# instead of XAML?

**A**: Yes! You can create UI entirely in C#, but XAML is generally preferred for better separation of concerns.

### Q: Why does Hot Reload not work sometimes?

**A**: Hot Reload works for XAML and some C# changes. It doesn't work for:
- Adding new files
- Changing constructors
- Some dependency injection changes

### Q: Do I need a Mac to develop iOS apps?

**A**: To develop and test, no. You can use the iOS simulator on Windows (limited). To publish to App Store, yes, you need a Mac for final build.

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **XAML** | Declarative markup for UI |
| **Code-Behind** | C# logic file (.xaml.cs) |
| **MauiProgram** | App startup configuration |
| **Hot Reload** | See changes without rebuilding |
| **Platforms Folder** | Platform-specific code |
| **Resources Folder** | Assets (images, fonts, etc.) |

---

## ✅ Checklist

Before moving to Lesson 2, make sure you can:

- [ ] Create a new MAUI project
- [ ] Run the app on Windows
- [ ] Run the app on Android emulator (optional)
- [ ] Use Hot Reload to update UI
- [ ] Add and modify basic controls (Label, Button, Entry)
- [ ] Handle button clicks in code-behind
- [ ] Understand the project structure

---

## 🚀 Next Steps

Great job creating your first MAUI app! In the next lesson, we'll dive deeper into **XAML Basics** and learn how to build beautiful, responsive user interfaces.

**Next Lesson**: [Lesson 2: XAML Basics](02_XAML_Basics.md)

---

## 📖 Additional Reading

- [Official .NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app)
- [MAUI Samples on GitHub](https://github.com/dotnet/maui-samples)
- [XAML Hot Reload Documentation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/hot-reload)
