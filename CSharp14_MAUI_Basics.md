# C# 14 and .NET MAUI Basics

## C# 14 Basics (Released November 2025 with .NET 10 LTS)

### Key New Features

**1. Extension Members** (Headline Feature)
Extension members allow you to add properties, events, and indexers to existing types, not just methods.

```csharp
// C# 13: Only extension methods
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
}

// C# 14: Extension properties, events, and indexers
public static class StringExtensions
{
    // Extension property
    public static bool IsEmpty { get => string.IsNullOrEmpty(this string? value); }
    
    // Extension event
    public static event EventHandler? Changed;
    
    // Extension indexer
    public static char? this this string s[int index] => 
        s.Length > index ? s[index] : null;
}
```

**2. Field Keywords for Properties**
Simplify property syntax with `field` keyword for auto-property backing fields.

```csharp
// C# 14: field keyword
public class Product
{
    public string Name 
    { 
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    }
    
    // Validate on set
    public decimal Price 
    {
        get => field;
        set => field = value > 0 ? value : throw new ArgumentOutOfRangeException();
    }
}
```

**3. Lambda Parameter Modifiers**
Use `ref`, `out`, `in`, `params` on lambda parameters without explicit types.

```csharp
// C# 14: Lambda with modifiers
var calculate = (ref int x, in int y, out int result) => 
{
    result = x + y;
    x = result;
};

// Usage
int a = 10, b = 20, result;
calculate(ref a, in b, out result); // a = 30, result = 30
```

**4. Params Collections**
Use any collection type with `params`, not just arrays.

```csharp
// C# 14: params with any collection
public void PrintProducts(params IEnumerable<Product> products)
{
    foreach (var product in products)
        Console.WriteLine(product.Name);
}

// Usage
PrintProducts(new List<Product> { p1, p2 });
PrintProducts(p1, p2, p3); // Works like array
```

**5. Async State**
Improved async/await with `AsyncState` for better performance.

```csharp
// C# 14: AsyncState for performance-critical async
public async Task<Product> GetProductAsync(int id, AsyncState state = default)
{
    // Reuse state machine for better performance
    await using var context = new DbContext();
    return await context.Products.FindAsync(id, state).ConfigureAwait(false);
}
```

---

## .NET MAUI Basics

### What is .NET MAUI?

**.NET MAUI** (Multi-platform App UI) is Microsoft's cross-platform framework for building native mobile and desktop applications with a single codebase.

**Supported Platforms:**
- iOS
- Android
- Windows (WinUI 3)
- macOS (Mac Catalyst)

### Core Architecture

```
.NET MAUI Application
├── App.xaml.cs (Entry Point)
├── MauiProgram.cs (Configuration & DI)
├── Dependency Injection Container
│   ├── Services (Database, Auth, API)
│   ├── ViewModels (Business Logic)
│   └── Views/Pages (XAML UI)
└── Platform Abstraction
    ├── .NET for Android
    ├── .NET for iOS
    ├── Windows SDK
    └── Mac Catalyst
```

### Key Concepts

#### 1. MauiProgram.cs - Application Entry Point
[`MauiProgram.CreateMauiApp()`](SweetShopMa/MauiProgram.cs:39) configures the app:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder
        .UseMauiApp<App>()              // Set main App class
        .ConfigureFonts(fonts => {      // Register fonts
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        })
        .Services.AddSingleton<DatabaseService>()  // Register services
        .AddSingleton<ShopViewModel>()
        .AddTransient<LoginPage>();
    
    return builder.Build();
}
```

#### 2. Dependency Injection
MAUI has built-in DI container with three lifetimes:

```csharp
// Singleton: One instance for entire app
.AddSingleton<DatabaseService>()  // Shared state

// Transient: New instance each time
.AddTransient<LoginPage>()  // Fresh page each navigation

// Scoped: One instance per scope (rarely used in MAUI)
```

#### 3. MVVM Pattern
Separation of concerns:

- **Model**: Data structures ([`Product.cs`](SweetShopMa/Models/Product.cs))
- **View**: XAML UI ([`ProductsPage.xaml`](SweetShopMa/Views/ProductsPage.xaml))
- **ViewModel**: Business logic ([`ProductManagementViewModel.cs`](SweetShopMa/ViewModels/ProductManagementViewModel.cs))

```csharp
// ViewModel with CommunityToolkit.Mvvm
public partial class ProductManagementViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Product> _products;
    
    [RelayCommand]
    private async Task AddProductAsync() => await _database.AddProductAsync();
}
```

#### 4. XAML UI
Declarative markup for UI:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SweetShopMa.Views.ProductsPage">
    
    <StackLayout Padding="20">
        <Label Text="Products" FontSize="24" />
        <CollectionView ItemsSource="{Binding Products}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <StackLayout>
                        <Label Text="{Binding Name}" />
                        <Label Text="{Binding Price, StringFormat='{0:C}'}" />
                    </StackLayout>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </StackLayout>
</ContentPage>
```

#### 5. Navigation
Shell-based routing:

```csharp
// Register routes in AppShell.xaml
<Routing.RegisterRoute Route="Products" 
    Type="local:ProductsPage" />

// Navigate
await Shell.Current.GoToAsync("//Products");
```

#### 6. Platform-Specific Code
Conditional compilation:

```csharp
#if WINDOWS
    // Windows-specific code
    .AddSingleton<IPrintService, WindowsPrintService>()
#else
    // Other platforms
    .AddSingleton<IPrintService, DefaultPrintService>()
#endif
```

### MAUI Project Structure

```
SweetShopMa/
├── MauiProgram.cs          # App configuration & DI
├── App.xaml.cs             # App entry point
├── AppShell.xaml           # Navigation structure
├── Models/                 # Data models
├── ViewModels/             # Business logic (MVVM)
├── Views/                  # XAML pages
├── Services/               # Business services
├── Resources/              # Fonts, images, strings
├── Platforms/              # Platform-specific code
│   ├── Android/
│   ├── iOS/
│   ├── Windows/
│   └── MacCatalyst/
└── Controls/               # Custom controls
```

### Key MAUI Features

| Feature | Description |
|---------|-------------|
| **Single Project** | One project for all platforms |
| **Hot Reload** | See UI changes instantly |
| **Built-in DI** | Microsoft.Extensions.DependencyInjection |
| **XAML Hot Reload** | Edit XAML while app runs |
| **Native Performance** | Uses native platform controls |
| **MAUI Community Toolkit** | Helpers, converters, behaviors |

### Getting Started Checklist

```
Install Visual Studio 2022
    ↓
Enable .NET MAUI workload
    ↓
Create new MAUI Project
    ↓
Understand MauiProgram.cs
    ↓
Learn MVVM Pattern
    ↓
Master XAML Basics
    ↓
Use CommunityToolkit.Mvvm
    ↓
Build First App
```

### Recommended Learning Path

1. **C# Fundamentals** → Async/await, LINQ, generics
2. **MAUI Basics** → MauiProgram, App lifecycle, XAML
3. **MVVM Pattern** → CommunityToolkit.Mvvm, data binding
4. **Services** → Dependency injection, platform-specific code
5. **Data** → SQLite, REST APIs, serialization
6. **Advanced** → Custom renderers, effects, animations

### From Your Project

Your [`SweetShopMa`](SweetShopMa/MauiProgram.cs:1) project demonstrates:

1. **Proper DI setup** with [`AddSingleton`](SweetShopMa/MauiProgram.cs:62) and [`AddTransient`](SweetShopMa/MauiProgram.cs:108)
2. **MVVM architecture** with ViewModels and Views
3. **Platform-specific services** with conditional compilation
4. **Localization support** with RTL (Right-to-Left) for Arabic
5. **SQLite integration** for local data storage
6. **Async/await patterns** throughout

### Additional Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [C# 14 What's New](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [MAUI Community Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
