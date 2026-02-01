# Lesson 6: Dependency Injection (DI) Made Simple

## 🎯 Learning Objectives

By the end of this lesson, you will:
- ✅ Understand what Dependency Injection (DI) is in plain English
- ✅ Learn why DI makes your code better and easier to maintain
- ✅ Master the three service lifetimes (Singleton, Transient, Scoped)
- ✅ Register and inject services like a pro
- ✅ Use DI with ViewModels and Pages
- ✅ Build a complete DI-based architecture from scratch

---

## 📖 What is Dependency Injection? (The Simple Explanation)

### 🏠 Real-World Analogy: The Restaurant Kitchen

Think of Dependency Injection like a restaurant kitchen:

**Without DI (Bad Practice):**
```
Chef: "I need to bake a cake!"
Chef: *Goes to the farm, milks the cow, gathers eggs, grinds wheat...*
Chef: "Now I can finally bake!"
```
**Problems:** 
- The chef has too many responsibilities
- If the farm is closed, the chef can't work
- Hard to test if the chef is good at baking

**With DI (Good Practice):**
```
Chef: "I need ingredients to bake a cake!"
Kitchen Manager: *Provides all the ingredients*
Chef: "Thanks! Now I can focus on baking!"
```
**Benefits:**
- Chef focuses on what they do best (baking)
- Kitchen Manager handles getting ingredients
- Easy to test the chef's baking skills
- Easy to swap ingredient suppliers

---

### 💻 In Programming Terms

**Dependency Injection (DI)** means: Instead of creating objects inside your class, you ask for them to be provided (injected) from outside.

**Without DI (Tight Coupling):**
```csharp
public class TodoViewModel
{
    private DatabaseService _database;
    
    public TodoViewModel()
    {
        // ❌ BAD: Creating dependency inside
        // Problems:
        // 1. Can't test without real database
        // 2. Can't switch to different database easily
        // 3. ViewModel is tightly coupled to DatabaseService
        _database = new DatabaseService();
    }
}
```

**With DI (Loose Coupling):**
```csharp
public class TodoViewModel
{
    private readonly IDatabaseService _database;
    
    // ✅ GOOD: Dependency provided from outside
    // Benefits:
    // 1. Can test with fake database
    // 2. Easy to switch implementations
    // 3. ViewModel is independent of database details
    public TodoViewModel(IDatabaseService database)
    {
        _database = database;
    }
}
```

### 🎯 Key Benefits Explained

| Benefit | What It Means | Why It Matters |
|---------|---------------|----------------|
| **Loose Coupling** | Classes don't depend on each other directly | Change one part without breaking others |
| **Easy Testing** | Can inject fake/test versions | Test without real database, API, etc. |
| **Flexibility** | Swap implementations easily | Change database, API, or service without rewriting code |
| **Better Organization** | Clear separation of concerns | Each class has one job |
| **Maintainability** | Easier to understand and modify | New developers can understand code faster |

---

## 🏗️ How DI Works in MAUI (Step by Step)

### The Big Picture

```
┌─────────────────────────────────────────────────────────────┐
│  1. MauiProgram.cs (The Kitchen Manager)                    │
│     └─> Registers all services (creates the menu)           │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  2. DI Container (The Pantry)                               │
│     └─> Manages service lifetimes (stores ingredients)      │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  3. Constructor Injection (The Chef's Request)              │
│     └─> ViewModels/Pages ask for services                   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  4. DI Container Provides (Delivery)                        │
│     └─> Automatically gives instances to constructors       │
└─────────────────────────────────────────────────────────────┘
```

### What Happens When Your App Starts?

```csharp
// This runs when your app starts
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // Step 1: Tell MAUI about your services
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddTransient<MainPageViewModel>();
        
        // Step 2: Build the app (creates the DI container)
        return builder.Build();
    }
}
```

### What Happens When You Navigate to a Page?

```csharp
// When MAUI creates MainPage:
// 1. MAUI sees MainPage needs MainPageViewModel
// 2. MAUI checks DI container for MainPageViewModel
// 3. MAUI sees MainPageViewModel needs IDatabaseService
// 4. MAUI provides IDatabaseService instance
// 5. MAUI creates MainPageViewModel with the database
// 6. MAUI creates MainPage with the view model

public MainPage(MainPageViewModel viewModel)  // MAUI injects this
{
    InitializeComponent();
    BindingContext = viewModel;
}
```

---

## 📦 Service Lifetimes: When Are Services Created?

### The Three Types Explained Simply

Think of service lifetimes like **borrowing books from a library**:

---

### 1. Singleton - One Copy Forever 📚

**Analogy:** The library has only ONE copy of a rare book. Everyone shares it.

**In Code:**
```csharp
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
```

**Behavior:**
- Created once when app starts
- Same instance used everywhere
- Never destroyed until app closes

**When to Use:**
✅ Database connections  
✅ Settings/configuration  
✅ Caching services  
✅ Logging services  
✅ Services with shared state  

**Example:**
```csharp
public class DatabaseService : IDatabaseService
{
    private readonly SQLiteConnection _connection;
    
    public DatabaseService()
    {
        // Created once, shared by all ViewModels
        _connection = new SQLiteConnection("todos.db");
        _connection.CreateTable<TodoItem>();
    }
    
    // All ViewModels use the same connection
    public List<TodoItem> GetTodos() => _connection.Table<TodoItem>().ToList();
}
```

**Why Singleton for Database?**
- You want ONE connection, not multiple connections fighting
- All ViewModels see the same data
- Efficient resource usage

---

### 2. Transient - New Copy Every Time 📄

**Analogy:** The library prints a NEW copy of a worksheet for each person who asks.

**In Code:**
```csharp
builder.Services.AddTransient<MainPageViewModel>();
```

**Behavior:**
- New instance created every time it's requested
- Each request gets a fresh copy
- Destroyed when no longer needed

**When to Use:**
✅ ViewModels (usually)  
✅ Stateless services  
✅ Lightweight calculations  
✅ Services with no shared state  

**Example:**
```csharp
public class AddTodoViewModel : ObservableObject
{
    // Each navigation gets a FRESH view model
    public AddTodoViewModel(IDatabaseService database)
    {
        // Clean state every time
        _database = database;
    }
    
    [ObservableProperty]
    private string _title = "";  // Starts fresh each time
}
```

**Why Transient for ViewModels?**
- Each page visit should start fresh
- No leftover data from previous visits
- Clean state for each user interaction

---

### 3. Scoped - One Copy Per Request 🎫

**Analogy:** Each customer gets their own shopping cart, but it's the same cart throughout their shopping trip.

**In Code:**
```csharp
builder.Services.AddScoped<RequestContext>();
```

**Behavior:**
- One instance per "scope"
- Same instance within that scope
- Different instances for different scopes

**When to Use:**
✅ Rarely used in mobile apps  
✅ More common in web applications  
✅ Per-request data in API calls  

**Note:** Scoped is rarely used in MAUI because mobile apps don't have "requests" like web apps.

---

### Lifetime Comparison Table

| Lifetime | Instances Created | Memory Usage | Thread Safety | Best For |
|----------|-------------------|--------------|---------------|----------|
| **Singleton** | 1 (entire app) | Low | ⚠️ Required | Shared state, connections |
| **Transient** | Many (per request) | Medium | ✅ Not needed | ViewModels, stateless services |
| **Scoped** | 1 per scope | Medium | ⚠️ May need | Request-scoped data (rare in MAUI) |

### Quick Decision Guide

```
Does the service need to share state across the app?
├─ YES → Use Singleton
└─ NO
    └─ Is it a ViewModel or lightweight service?
        ├─ YES → Use Transient
        └─ NO → Use Singleton (most services)
```

---

## 🔧 Setting Up DI in MAUI (Complete Tutorial)

### Without DI (Bad Practice)

```csharp
public class TodoViewModel
{
    private DatabaseService _database;
    
    public TodoViewModel()
    {
        // Hard dependency! Can't test or change easily
        _database = new DatabaseService();
    }
}
```

**Problems:**
- Tight coupling between classes
- Hard to test (can't mock DatabaseService)
- Difficult to change implementation
- Violates Single Responsibility Principle

### With DI (Good Practice)

```csharp
public class TodoViewModel
{
    private readonly DatabaseService _database;
    
    // Dependencies injected through constructor
    public TodoViewModel(DatabaseService database)
    {
        _database = database;
    }
}
```

**Benefits:**
✅ Loose coupling  
✅ Easy testing (can inject mocks)  
✅ Flexible implementation changes  
✅ Follows SOLID principles  

---

## 🏗️ MAUI Built-in DI Container

MAUI includes Microsoft.Extensions.DependencyInjection for free!

### How DI Works in MAUI

```
1. Register services in MauiProgram.cs
        ↓
2. Container manages service lifetimes
        ↓
3. Inject services into constructors
        ↓
4. Container provides instances automatically
```

---

## 📦 Service Lifetimes

MAUI DI supports three service lifetimes. Choosing the right one is critical!

### 1. Singleton - One Instance Forever

**Creates**: One instance for entire app lifetime  
**Use for**: Stateful services, caches, configuration  

```csharp
// In MauiProgram.cs
builder.Services.AddSingleton<DatabaseService>();
```

**When to use:**
- Database connections
- Configuration/settings
- Caching services
- Logging services
- Services with shared state

**Example:**
```csharp
public class DatabaseService
{
    private readonly SQLiteConnection _connection;
    
    public DatabaseService()
    {
        _connection = new SQLiteConnection("todos.db");
        _connection.CreateTable<TodoItem>();
    }
    
    // All ViewModels share same connection
}
```

### 2. Transient - New Instance Every Time

**Creates**: New instance each time it's requested  
**Use for**: Stateless services, lightweight objects  

```csharp
// In MauiProgram.cs
builder.Services.AddTransient<AddTodoViewModel>();
```

**When to use:**
- ViewModels (usually)
- Stateless services
- Lightweight calculations
- Services with no shared state

**Example:**
```csharp
public class AddTodoViewModel
{
    // Each page gets its own instance
    public AddTodoViewModel(DatabaseService database)
    {
        // Fresh instance for each navigation
    }
}
```

### 3. Scoped - One Instance Per Scope

**Creates**: One instance per scope (rarely used in MAUI)  
**Use for**: Request-scoped data (web apps mostly)  

```csharp
// In MauiProgram.cs
builder.Services.AddScoped<RequestContext>();
```

**When to use:**
- Rarely used in mobile apps
- More common in web applications
- Per-request data in API calls

### Lifetime Comparison

| Lifetime | Instances Created | When to Use | Example |
|----------|-------------------|-------------|---------|
| **Singleton** | 1 (entire app) | Shared state, connections | DatabaseService, SettingsService |
| **Transient** | Many (per request) | Stateless, lightweight | ViewModels, converters |
| **Scoped** | 1 per scope | Request-scoped data | RequestContext (rare in MAUI) |

---

## 🔧 Setting Up DI in MAUI

### Step 1: Create Your Services

**What is a Service?**
A service is a class that does something specific for your app - like saving data, making API calls, or handling navigation.

**First, Create an Interface (The Contract):**
```csharp
// Services/IDatabaseService.cs
// An interface is like a contract - it says WHAT a service can do
public interface IDatabaseService
{
    void Initialize();
    List<TodoItem> GetTodos();
    void AddTodo(TodoItem todo);
    void DeleteTodo(int id);
}
```

**Why use an interface?**
- Makes testing easier (can create fake versions)
- Allows swapping implementations
- Defines clear contract between parts of your app

**Then, Create the Implementation (The Real Thing):**
```csharp
// Services/DatabaseService.cs
// This is the actual code that does the work
public class DatabaseService : IDatabaseService
{
    private readonly SQLiteConnection _database;
    
    public DatabaseService()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "todos.db");
        _database = new SQLiteConnection(path);
        Initialize();
    }
    
    public void Initialize()
    {
        _database.CreateTable<TodoItem>();
    }
    
    public List<TodoItem> GetTodos() => _database.Table<TodoItem>().ToList();
    
    public void AddTodo(TodoItem todo) => _database.Insert(todo);
    
    public void DeleteTodo(int id) => _database.Delete<TodoItem>(id);
}
```

---

### Step 2: Register Services in MauiProgram.cs

**MauiProgram.cs is like your app's "setup file"** - it runs once when your app starts.

```csharp
using Microsoft.Extensions.Logging;
using MyApp.Services;
using MyApp.ViewModels;
using MyApp.Views;

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
        
        // 🎯 THIS IS THE IMPORTANT PART!
        // Register services so DI can inject them
        builder.Services
            // Singleton: One instance for entire app
            .AddSingleton<IDatabaseService, DatabaseService>()
            .AddSingleton<ISettingsService, SettingsService>()
            
            // Transient: New instance each time
            .AddTransient<MainPageViewModel>()
            .AddTransient<AddTodoViewModel>()
            .AddTransient<TodoDetailsViewModel>()
            
            // Register pages so MAUI can inject ViewModels
            .AddTransient<MainPage>()
            .AddTransient<AddTodoPage>()
            .AddTransient<TodoDetailsPage>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        return builder.Build();
    }
}
```

**What's happening here?**
- `AddSingleton<IDatabaseService, DatabaseService>()` tells MAUI: "When someone asks for IDatabaseService, give them DatabaseService (same instance every time)"
- `AddTransient<MainPageViewModel>()` tells MAUI: "When someone asks for MainPageViewModel, create a new one each time"

---

### Step 3: Inject Services into ViewModels

**Constructor Injection** - The DI way to get services!

```csharp
// ViewModels/MainPageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Services;

namespace MyApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    
    // ✅ MAGIC HAPPENS HERE!
    // MAUI automatically provides IDatabaseService
    // You don't create it - DI does it for you!
    public MainPageViewModel(IDatabaseService database)
    {
        _database = database;
        LoadTodos();
    }
    
    [ObservableProperty]
    private ObservableCollection<TodoItem> _todos = new();
    
    [RelayCommand]
    private async Task AddTodoAsync()
    {
        await Shell.Current.GoToAsync("addtodo");
    }
    
    [RelayCommand]
    private void LoadTodos()
    {
        var todos = _database.GetTodos();
        Todos.Clear();
        foreach (var todo in todos)
        {
            Todos.Add(todo);
        }
    }
}
```

**What just happened?**
1. MAUI sees MainPage needs MainPageViewModel
2. MAUI checks: "What does MainPageViewModel need?"
3. MAUI sees: "It needs IDatabaseService"
4. MAUI looks in DI container: "Do I have IDatabaseService?"
5. MAUI finds: "Yes! I registered DatabaseService"
6. MAUI creates DatabaseService (if needed) and injects it
7. MAUI creates MainPageViewModel with the database
8. MAUI creates MainPage with the view model

**All automatic! No `new` keyword needed!**

---

### Step 4: Inject ViewModels into Pages

```csharp
// Views/MainPage.xaml.cs
namespace MyApp.Views;

public partial class MainPage : ContentPage
{
    // MAUI injects the ViewModel here!
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

**Why inject the ViewModel into the Page?**
- MAUI handles creating the ViewModel with all its dependencies
- You don't need to manually create anything
- Everything is wired up automatically

---

## 🎯 Complete DI Example: Todo App (Step-by-Step)

Let's build a complete app using DI from scratch!

### Project Structure

```
MyApp/
├── Services/
│   ├── IDatabaseService.cs          (interface)
│   ├── DatabaseService.cs           (implementation)
│   ├── INavigationService.cs        (interface)
│   └── NavigationService.cs         (implementation)
├── ViewModels/
│   ├── TodoListViewModel.cs
│   └── AddTodoViewModel.cs
├── Views/
│   ├── TodoListPage.xaml
│   └── AddTodoPage.xaml
└── MauiProgram.cs                    (DI registration)
```

---

### Step 1: Create the Services

**Database Service:**
```csharp
// Services/IDatabaseService.cs
public interface IDatabaseService
{
    List<TodoItem> GetTodos();
    TodoItem GetTodo(int id);
    void AddTodo(TodoItem todo);
    void UpdateTodo(TodoItem todo);
    void DeleteTodo(int id);
}

// Services/DatabaseService.cs
public class DatabaseService : IDatabaseService
{
    // In-memory storage (use SQLite in real app)
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;
    
    public List<TodoItem> GetTodos() => _todos.ToList();
    
    public TodoItem GetTodo(int id) => _todos.FirstOrDefault(t => t.Id == id);
    
    public void AddTodo(TodoItem todo)
    {
        todo.Id = _nextId++;
        todo.CreatedDate = DateTime.Now;
        _todos.Add(todo);
    }
    
    public void UpdateTodo(TodoItem todo)
    {
        var index = _todos.FindIndex(t => t.Id == todo.Id);
        if (index >= 0)
        {
            _todos[index] = todo;
        }
    }
    
    public void DeleteTodo(int id)
    {
        var todo = GetTodo(id);
        if (todo != null)
        {
            _todos.Remove(todo);
        }
    }
}
```

**Navigation Service:**
```csharp
// Services/INavigationService.cs
public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync(string route, Dictionary<string, object> parameters);
    Task GoBackAsync();
}

// Services/NavigationService.cs
public class NavigationService : INavigationService
{
    public Task GoToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }
    
    public Task GoToAsync(string route, Dictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync(route, parameters);
    }
    
    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}
```

---

### Step 2: Create the ViewModels

**Todo List ViewModel:**
```csharp
// ViewModels/TodoListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class TodoListViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    private readonly INavigationService _navigation;
    
    // DI injects both services automatically!
    public TodoListViewModel(IDatabaseService database, INavigationService navigation)
    {
        _database = database;
        _navigation = navigation;
        LoadTodos();
    }
    
    [ObservableProperty]
    private ObservableCollection<TodoItem> _todos = new();
    
    [RelayCommand]
    private async Task AddTodoAsync()
    {
        await _navigation.GoToAsync("addtodo");
    }
    
    [RelayCommand]
    private async Task SelectTodoAsync(TodoItem todo)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Todo", todo }
        };
        await _navigation.GoToAsync("tododetails", parameters);
    }
    
    [RelayCommand]
    private void LoadTodos()
    {
        var todos = _database.GetTodos();
        Todos.Clear();
        foreach (var todo in todos)
        {
            Todos.Add(todo);
        }
    }
}
```

**Add Todo ViewModel:**
```csharp
// ViewModels/AddTodoViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class AddTodoViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    private readonly INavigationService _navigation;
    
    public AddTodoViewModel(IDatabaseService database, INavigationService navigation)
    {
        _database = database;
        _navigation = navigation;
    }
    
    [ObservableProperty]
    private string _title;
    
    [ObservableProperty]
    private string _description;
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Error", "Title is required", "OK");
            return;
        }
        
        var todo = new TodoItem
        {
            Title = Title,
            Description = Description,
            IsCompleted = false
        };
        
        _database.AddTodo(todo);
        await _navigation.GoBackAsync();
    }
    
    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.GoBackAsync();
    }
}
```

---

### Step 3: Register Everything in MauiProgram.cs

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });
    
    // 🎯 Register all services
    builder.Services
        // Services (Singleton - one instance for app)
        .AddSingleton<IDatabaseService, DatabaseService>()
        .AddSingleton<INavigationService, NavigationService>()
        
        // ViewModels (Transient - new instance each time)
        .AddTransient<TodoListViewModel>()
        .AddTransient<AddTodoViewModel>()
        .AddTransient<TodoDetailsViewModel>()
        
        // Pages (Transient - new instance each time)
        .AddTransient<TodoListPage>()
        .AddTransient<AddTodoPage>()
        .AddTransient<TodoDetailsPage>();
    
#if DEBUG
    builder.Logging.AddDebug();
#endif
    
    return builder.Build();
}
```

---

### Step 4: Create the Pages

**Todo List Page:**
```xml
<!-- Views/TodoListPage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.Views.TodoListPage"
             Title="My Todos">
    
    <StackLayout Padding="20">
        <Button Text="Add Todo" 
                Command="{Binding AddTodoCommand}" />
        
        <CollectionView ItemsSource="{Binding Todos}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <StackLayout Padding="10">
                        <Label Text="{Binding Title}" 
                               FontSize="18" />
                        <Label Text="{Binding Description}" 
                               FontSize="14" 
                               TextColor="Gray" />
                    </StackLayout>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </StackLayout>
</ContentPage>
```

```csharp
// Views/TodoListPage.xaml.cs
namespace MyApp.Views;

public partial class TodoListPage : ContentPage
{
    // DI injects the ViewModel!
    public TodoListPage(TodoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

**Add Todo Page:**
```xml
<!-- Views/AddTodoPage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.Views.AddTodoPage"
             Title="Add Todo">
    
    <StackLayout Padding="20" Spacing="15">
        <Entry Placeholder="Title" 
               Text="{Binding Title}" />
        
        <Editor Placeholder="Description" 
                Text="{Binding Description}"
                HeightRequest="100" />
        
        <StackLayout Orientation="Horizontal" Spacing="10">
            <Button Text="Save" 
                    Command="{Binding SaveCommand}"
                    HorizontalOptions="FillAndExpand" />
            <Button Text="Cancel" 
                    Command="{Binding CancelCommand}"
                    HorizontalOptions="FillAndExpand" />
        </StackLayout>
    </StackLayout>
</ContentPage>
```

```csharp
// Views/AddTodoPage.xaml.cs
namespace MyApp.Views;

public partial class AddTodoPage : ContentPage
{
    public AddTodoPage(AddTodoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

---

## 🎓 Advanced DI Concepts

### Constructor Injection vs. Property Injection

**Constructor Injection (Recommended):**
```csharp
public class MyViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    
    // ✅ GOOD: Required dependencies in constructor
    public MyViewModel(IDatabaseService database)
    {
        _database = database;
    }
}
```

**Property Injection (Rare):**
```csharp
public class MyViewModel : ObservableObject
{
    // ❌ AVOID: Optional dependencies as properties
    public IDatabaseService Database { get; set; }
}
```

**Why Constructor Injection is Better:**
- Makes dependencies clear and required
- Can't create object without dependencies
- Easier to test
- More explicit

---

### Resolving Multiple Implementations

What if you have multiple implementations of an interface?

```csharp
// Register with different names/keys
builder.Services.AddSingleton<IDatabaseService, SqliteDatabaseService>("sqlite");
builder.Services.AddSingleton<IDatabaseService, MockDatabaseService>("mock");

// Resolve specific implementation
var sqliteDb = app.Services.GetRequiredService<IDatabaseService>("sqlite");
```

---

### Factory Pattern

Sometimes you need to create services dynamically:

```csharp
// Register factory
builder.Services.AddTransient<Func<string, IDatabaseService>>(sp => 
    key => key switch
    {
        "sqlite" => sp.GetRequiredService<SqliteDatabaseService>(),
        "mock" => sp.GetRequiredService<MockDatabaseService>(),
        _ => throw new ArgumentException($"Unknown database: {key}")
    });

// Use in ViewModel
public class MyViewModel
{
    private readonly Func<string, IDatabaseService> _dbFactory;
    
    public MyViewModel(Func<string, IDatabaseService> dbFactory)
    {
        _dbFactory = dbFactory;
        var db = _dbFactory("sqlite");
    }
}
```

---

## 🐛 Common DI Mistakes & How to Fix Them

### Mistake 1: Creating Services with `new`

```csharp
// ❌ WRONG: Defeats the purpose of DI
public class MyViewModel
{
    private readonly DatabaseService _database;
    
    public MyViewModel()
    {
        _database = new DatabaseService();  // Don't do this!
    }
}

// ✅ CORRECT: Let DI handle it
public class MyViewModel
{
    private readonly IDatabaseService _database;
    
    public MyViewModel(IDatabaseService database)
    {
        _database = database;
    }
}
```

### Mistake 2: Wrong Service Lifetime

```csharp
// ❌ WRONG: ViewModel as Singleton (state issues!)
builder.Services.AddSingleton<MyViewModel>();

// ✅ CORRECT: ViewModel as Transient (fresh each time)
builder.Services.AddTransient<MyViewModel>();

// ❌ WRONG: Database as Transient (multiple connections!)
builder.Services.AddTransient<DatabaseService>();

// ✅ CORRECT: Database as Singleton (one connection)
builder.Services.AddSingleton<DatabaseService>();
```

### Mistake 3: Not Registering Services

```csharp
// ❌ ERROR: Service not registered!
public MyViewModel(IDatabaseService database) { }

// ✅ FIX: Register the service
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
```

### Mistake 4: Circular Dependencies

```csharp
// ❌ ERROR: Circular dependency!
public class ServiceA
{
    public ServiceA(ServiceB serviceB) { }
}

public class ServiceB
{
    public ServiceB(ServiceA serviceA) { }
}

// ✅ FIX: Redesign to avoid circular dependency
public class ServiceA
{
    public ServiceA() { }
}

public class ServiceB
{
    public ServiceB(ServiceA serviceA) { }
}
```

---

## 📚 DI Best Practices Checklist

### ✅ DO:
- Use interfaces for services
- Inject dependencies through constructors
- Use Singleton for stateful services (database, settings)
- Use Transient for ViewModels and stateless services
- Register all services in MauiProgram.cs
- Keep services focused (single responsibility)
- Use meaningful names for services

### ❌ DON'T:
- Create services with `new` keyword
- Use Singleton for ViewModels
- Inject too many dependencies (>5 is a code smell)
- Use property injection
- Forget to register services
- Create circular dependencies
- Put business logic in constructors

---

## 🎯 Summary: DI in 60 Seconds

**What is DI?**
A way to provide dependencies to classes from outside instead of creating them inside.

**How does it work?**
1. Register services in MauiProgram.cs
2. DI container manages service lifetimes
3. Inject services through constructors
4. MAUI automatically provides instances

**Why use it?**
- Loose coupling (easy to change)
- Easy testing (can inject mocks)
- Better organization (clear responsibilities)
- Maintainable code (easier to understand)

**Three Lifetimes:**
- **Singleton**: One instance forever (database, settings)
- **Transient**: New instance each time (ViewModels)
- **Scoped**: One per scope (rare in MAUI)

**The Pattern:**
```
Interface → Implementation → Register in MauiProgram → Inject in Constructor
```

---

## 🚀 Next Steps

Now that you understand DI, you're ready to:
- ✅ Build testable, maintainable apps
- ✅ Use advanced MAUI features
- ✅ Create scalable architectures
- ✅ Write better code

**Next Lesson:** Platform-Specific Code - Learn how to access platform-specific features while keeping your code cross-platform!

---

## 🎯 Complete DI Example: Todo App

Let's build a complete app using DI.

### Services

```csharp
// Services/IDatabaseService.cs
public interface IDatabaseService
{
    List<TodoItem> GetTodos();
    TodoItem GetTodo(int id);
    void AddTodo(TodoItem todo);
    void UpdateTodo(TodoItem todo);
    void DeleteTodo(int id);
}

// Services/DatabaseService.cs
public class DatabaseService : IDatabaseService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;
    
    public List<TodoItem> GetTodos() => _todos.ToList();
    
    public TodoItem GetTodo(int id) => _todos.FirstOrDefault(t => t.Id == id);
    
    public void AddTodo(TodoItem todo)
    {
        todo.Id = _nextId++;
        todo.CreatedDate = DateTime.Now;
        _todos.Add(todo);
    }
    
    public void UpdateTodo(TodoItem todo)
    {
        var index = _todos.FindIndex(t => t.Id == todo.Id);
        if (index >= 0)
        {
            _todos[index] = todo;
        }
    }
    
    public void DeleteTodo(int id)
    {
        var todo = GetTodo(id);
        if (todo != null)
        {
            _todos.Remove(todo);
        }
    }
}

// Services/INavigationService.cs
public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync(string route, Dictionary<string, object> parameters);
    Task GoBackAsync();
}

// Services/NavigationService.cs
public class NavigationService : INavigationService
{
    public Task GoToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }
    
    public Task GoToAsync(string route, Dictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync(route, parameters);
    }
    
    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}
```

### ViewModels

```csharp
// ViewModels/TodoListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class TodoListViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    private readonly INavigationService _navigation;
    
    public TodoListViewModel(IDatabaseService database, INavigationService navigation)
    {
        _database = database;
        _navigation = navigation;
        LoadTodos();
    }
    
    [ObservableProperty]
    private ObservableCollection<TodoItem> _todos = new();
    
    [RelayCommand]
    private async Task AddTodoAsync()
    {
        await _navigation.GoToAsync("addtodo");
    }
    
    [RelayCommand]
    private async Task SelectTodoAsync(TodoItem todo)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Todo", todo }
        };
        await _navigation.GoToAsync("tododetails", parameters);
    }
    
    private void LoadTodos()
    {
        var todos = _database.GetTodos();
        Todos.Clear();
        foreach (var todo in todos)
        {
            Todos.Add(todo);
        }
    }
}

// ViewModels/AddTodoViewModel.cs
public partial class AddTodoViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    private readonly INavigationService _navigation;
    
    public AddTodoViewModel(IDatabaseService database, INavigationService navigation)
    {
        _database = database;
        _navigation = navigation;
    }
    
    [ObservableProperty]
    private string _title;
    
    [ObservableProperty]
    private string _description;
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Error", "Title is required", "OK");
            return;
        }
        
        var todo = new TodoItem
        {
            Title = Title,
            Description = Description,
            IsCompleted = false
        };
        
        _database.AddTodo(todo);
        await _navigation.GoBackAsync();
    }
    
    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.GoBackAsync();
    }
}
```

### MauiProgram.cs

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts => { /* ... */ });
    
    // Services
    builder.Services
        .AddSingleton<IDatabaseService, DatabaseService>()
        .AddSingleton<INavigationService, NavigationService>();
    
    // ViewModels
    builder.Services
        .AddTransient<TodoListViewModel>()
        .AddTransient<AddTodoViewModel>()
        .AddTransient<TodoDetailsViewModel>();
    
    // Pages
    builder.Services
        .AddTransient<TodoListPage>()
        .AddTransient<AddTodoPage>()
        .AddTransient<TodoDetailsPage>();
    
    return builder.Build();
}
```

---

## 🎨 Advanced DI Patterns

### Factory Pattern

Create instances with runtime parameters:

```csharp
// Services/IViewModelFactory.cs
public interface IViewModelFactory
{
    T Create<T>() where T : class;
}

// Services/ViewModelFactory.cs
public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public T Create<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
```

### Lazy Initialization

Delay service creation until needed:

```csharp
public class MyViewModel
{
    private readonly Lazy<IHeavyService> _heavyService;
    
    public MyViewModel(Lazy<IHeavyService> heavyService)
    {
        _heavyService = heavyService;
    }
    
    public void DoWork()
    {
        // Service created only when first accessed
        _heavyService.Value.DoSomething();
    }
}
```

### Multiple Implementations

Register and resolve multiple implementations:

```csharp
// Register multiple implementations
builder.Services.AddSingleton<ILogger, ConsoleLogger>();
builder.Services.AddSingleton<ILogger, FileLogger>();

// Resolve all implementations
public class MyViewModel
{
    private readonly IEnumerable<ILogger> _loggers;
    
    public MyViewModel(IEnumerable<ILogger> loggers)
    {
        _loggers = loggers;
    }
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Shopping Cart with DI

Create a shopping cart app using DI:

**Services:**
- `IProductService` - Get products
- `ICartService` - Manage cart (add, remove, clear, total)
- `INavigationService` - Navigate between pages

**ViewModels:**
- `ProductListViewModel` - Display products
- `CartViewModel` - Show cart items and total
- `CheckoutViewModel` - Checkout form

**Requirements:**
- Register all services as Singleton (except ViewModels)
- Register ViewModels as Transient
- Inject services into ViewModels
- Navigate between pages using INavigationService

### Exercise 2: Settings Service

Create a settings service with DI:

**Service:**
```csharp
public interface ISettingsService
{
    string Username { get; set; }
    bool NotificationsEnabled { get; set; }
    string Theme { get; set; } // Light, Dark, Auto
    void Save();
    void Load();
}
```

**Requirements:**
- Implement using Preferences API
- Register as Singleton
- Inject into multiple ViewModels
- Persist settings across app restarts
- Create settings page to modify settings

### Exercise 3: Logger Service

Create a logging service with DI:

**Service:**
```csharp
public interface ILogger
{
    void Log(string message);
    void LogError(string error);
    void LogWarning(string warning);
    List<string> GetLogs();
}
```

**Requirements:**
- Implement with in-memory list
- Register as Singleton
- Inject into ViewModels
- Add logging to all ViewModels
- Create logs page to view all logs
- Clear logs command

---

## ❓ Common Beginner Questions

### Q: Why not just use `new` to create services?

**A**: DI provides:
- Testability (can inject mocks)
- Flexibility (easy to swap implementations)
- Lifetime management (automatic disposal)
- Centralized configuration

### Q: When should I use Singleton vs Transient?

**A**: 
- **Singleton**: Services with state or expensive to create
- **Transient**: Stateless services or ViewModels

### Q: Can I inject services into code-behind?

**A**: Yes, but it's better to inject into ViewModels. Code-behind should only handle UI-specific logic.

### Q: How do I resolve services without constructor injection?

**A**: Use `IServiceProvider`:
```csharp
public class MyViewModel
{
    private readonly IServiceProvider _services;
    
    public MyViewModel(IServiceProvider services)
    {
        _services = services;
    }
    
    public void DoSomething()
    {
        var service = _services.GetRequiredService<IMyService>();
        service.DoWork();
    }
}
```

### Q: What if a service needs another service?

**A**: DI handles this automatically! Just inject the dependency:
```csharp
public class MyService : IMyService
{
    private readonly IDatabaseService _database;
    
    public MyService(IDatabaseService database)
    {
        _database = database;
    }
}
```

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **DI** | Inject dependencies instead of creating them |
| **Singleton** | One instance for entire app |
| **Transient** | New instance each time |
| **Scoped** | One instance per scope |
| **Constructor Injection** | Pass dependencies through constructor |
| **IServiceProvider** | Service locator (use sparingly) |

---

## ✅ Checklist

Before moving to Lesson 7, make sure you can:

- [ ] Explain what DI is and why it's useful
- [ ] Understand the three service lifetimes
- [ ] Register services in MauiProgram.cs
- [ ] Inject services into ViewModels and Pages
- [ ] Choose correct lifetime for services
- [ ] Build a complete DI-based architecture
- [ ] Use DI for navigation and data access

---

## 🚀 Next Steps

Great job mastering Dependency Injection! In the next lesson, we'll learn about **Platform-Specific Code** and how to handle differences between Android, iOS, Windows, and macOS.

**Next Lesson**: [Lesson 7: Platform-Specific Code](07_Platform_Specific_Code.md)

---

## 📖 Additional Reading

- [Dependency Injection in MAUI](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection/)
- [Microsoft.Extensions.DI](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Service Lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Best Practices for DI](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-best-practices)
