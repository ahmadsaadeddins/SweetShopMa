# Lesson 3: MVVM Pattern

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand what MVVM is and why it's important
- Learn the three components: Model, View, ViewModel
- Implement MVVM using CommunityToolkit.Mvvm
- Use data binding to connect UI to ViewModel
- Create commands to handle user actions
- Build a complete MVVM application

---

## 📖 What is MVVM?

**MVVM** (Model-View-ViewModel) is an architectural pattern that separates your app into three distinct layers. This makes your code more organized, testable, and maintainable.

### Why Use MVVM?

✅ **Separation of Concerns** - UI separate from business logic  
✅ **Testable** - Can test ViewModels without UI  
✅ **Maintainable** - Easy to find and fix bugs  
✅ **Reusable** - ViewModels can be used with different Views  
✅ **Team Collaboration** - Designers work on XAML, developers on C#  

### MVVM Architecture

```
┌─────────────────────────────────────────┐
│              VIEW (XAML)                │
│  - User Interface                       │
│  - Data Bindings                        │
│  - Commands                             │
└──────────────┬──────────────────────────┘
               │ Data Binding
               │
┌──────────────▼──────────────────────────┐
│           VIEWMODEL (C#)                │
│  - Business Logic                       │
│  - Properties (Observable)              │
│  - Commands                             │
│  - No UI code!                          │
└──────────────┬──────────────────────────┘
               │
               │
┌──────────────▼──────────────────────────┐
│            MODEL (C#)                   │
│  - Data Structures                      │
│  - Business Objects                     │
│  - Validation Logic                     │
└─────────────────────────────────────────┘
```

### The Three Components

#### 1. **Model** - The Data

Represents your data and business logic.

```csharp
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

#### 2. **View** - The UI

The XAML file that defines what the user sees.

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="MyApp.Views.TodoPage">
    
    <ListView ItemsSource="{Binding Todos}">
        <!-- UI code here -->
    </ListView>
    
</ContentPage>
```

#### 3. **ViewModel** - The Bridge

Connects the Model to the View. Contains properties and commands.

```csharp
public class TodoViewModel
{
    public ObservableCollection<TodoItem> Todos { get; set; }
    public ICommand AddTodoCommand { get; set; }
    
    // Business logic here
}
```

---

## 🔧 Setting Up MVVM in MAUI

### Step 1: Install CommunityToolkit.Mvvm

This package provides helpers that make MVVM much easier.

1. Right-click your project → "Manage NuGet Packages"
2. Search for "CommunityToolkit.Mvvm"
3. Install the latest version

### Step 2: Create Your First ViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    // Observable property (notifies UI when changed)
    [ObservableProperty]
    private string _name = "World";

    // Command (handles button clicks)
    [RelayCommand]
    private void Greet()
    {
        Name = "MAUI Developer";
    }
}
```

**What's happening here?**
- `ObservableObject` - Base class that implements INotifyPropertyChanged
- `[ObservableProperty]` - Auto-generates property with notification
- `[RelayCommand]` - Auto-generates ICommand for methods
- `partial class` - Required for source generators to work

### Step 3: Connect View to ViewModel

**Option A: In XAML (Recommended)**

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.MainPage"
             x:DataType="viewmodels:MainPageViewModel">
    
    <ContentPage.BindingContext>
        <viewmodels:MainPageViewModel />
    </ContentPage.BindingContext>
    
    <VerticalStackLayout Padding="20" Spacing="10">
        <Label Text="{Binding Name}" 
               FontSize="24"
               HorizontalOptions="Center" />
        
        <Button Text="Greet Me"
                Command="{Binding GreetCommand}" />
    </VerticalStackLayout>
    
</ContentPage>
```

**Option B: In Code-Behind**

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainPageViewModel();
    }
}
```

---

## 📊 Understanding Data Binding

Data binding connects UI properties to ViewModel properties automatically.

### Binding Modes

| Mode | Description | Example |
|------|-------------|---------|
| **OneWay** | ViewModel → View only | Displaying data |
| **TwoWay** | ViewModel ↔ View | Entry, Checkbox, etc. |
| **OneWayToSource** | View → ViewModel only | Rarely used |
| **Default** | Automatic based on control | Most common |

### Examples

```xml
<!-- OneWay: Display text from ViewModel -->
<Label Text="{Binding UserName}" />

<!-- TwoWay: Entry updates ViewModel, ViewModel updates Entry -->
<Entry Text="{Binding UserName, Mode=TwoWay}" />

<!-- OneWayToSource: Only send changes to ViewModel -->
<CheckBox IsChecked="{Binding IsActive, Mode=OneWayToSource}" />

<!-- Default: Let MAUI decide -->
<Switch IsToggled="{Binding NotificationsEnabled}" />
```

---

## 🎯 Observable Properties

Properties that automatically notify the UI when they change.

### Basic Observable Property

```csharp
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name;

    // Auto-generates:
    // public string Name
    // {
    //     get => _name;
    //     set => SetProperty(ref _name, value);
    // }
}
```

### Using Observable Properties

```xml
<Label Text="{Binding Name}" />
```

When `Name` changes in ViewModel, the Label automatically updates!

### Multiple Properties

```csharp
public partial class CounterViewModel : ObservableObject
{
    [ObservableProperty]
    private int _count = 0;

    [ObservableProperty]
    private string _message = "Click the button!";

    [RelayCommand]
    private void Increment()
    {
        Count++;
        Message = $"Clicked {Count} times";
    }
}
```

---

## 🔘 Commands

Commands handle user actions without code-behind.

### Relay Command

```csharp
public partial class MyViewModel : ObservableObject
{
    [RelayCommand]
    private void Save()
    {
        // Save logic here
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // Async operations
        await Task.Delay(1000);
    }
}
```

### Command with Parameters

```csharp
[RelayCommand]
private void DeleteItem(TodoItem item)
{
    Todos.Remove(item);
}
```

```xml
<Button Text="Delete"
        Command="{Binding DeleteItemCommand}"
        CommandParameter="{Binding .}" />
```

### CanExecute (Enable/Disable Commands)

```csharp
[ObservableProperty]
private string _name;

[RelayCommand(CanExecute = nameof(CanSave))]
private void Save()
{
    // Save logic
}

private bool CanSave() => !string.IsNullOrWhiteSpace(Name);

// Call this when Name changes to re-evaluate CanExecute
partial void OnNameChanged(string value)
{
    SaveCommand.NotifyCanExecuteChanged();
}
```

```xml
<Button Text="Save"
        Command="{Binding SaveCommand}" />
<!-- Button automatically enabled/disabled based on CanSave() -->
```

---

## 🏗️ Complete MVVM Example: Todo App

Let's build a complete MVVM application.

### Step 1: Create the Model

```csharp
// Models/TodoItem.cs
namespace MyApp.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
```

### Step 2: Create the ViewModel

```csharp
// ViewModels/TodoViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class TodoViewModel : ObservableObject
{
    // Observable collection (notifies UI when items added/removed)
    public ObservableCollection<TodoItem> Todos { get; } = new();

    [ObservableProperty]
    private string _newTodoTitle;

    [ObservableProperty]
    private string _appTitle = "My Todo App";

    [RelayCommand]
    private void AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTodoTitle))
            return;

        var todo = new TodoItem
        {
            Id = Todos.Count + 1,
            Title = NewTodoTitle,
            IsCompleted = false
        };

        Todos.Add(todo);
        NewTodoTitle = string.Empty;
    }

    [RelayCommand]
    private void DeleteTodo(TodoItem todo)
    {
        Todos.Remove(todo);
    }

    [RelayCommand]
    private void ToggleTodo(TodoItem todo)
    {
        todo.IsCompleted = !todo.IsCompleted;
    }

    [RelayCommand]
    private void ClearCompleted()
    {
        var completed = Todos.Where(t => t.IsCompleted).ToList();
        foreach (var todo in completed)
        {
            Todos.Remove(todo);
        }
    }
}
```

### Step 3: Create the View

```xml
<!-- Views/TodoPage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.TodoPage"
             x:DataType="viewmodels:TodoViewModel"
             Title="{Binding AppTitle}">

    <Grid RowDefinitions="Auto, *, Auto" Padding="20">
        
        <!-- Header -->
        <VerticalStackLayout Grid.Row="0" Spacing="10">
            <Label Text="Add New Todo"
                   FontSize="18"
                   FontAttributes="Bold" />
            
            <HorizontalStackLayout Spacing="10">
                <Entry Placeholder="What needs to be done?"
                       Text="{Binding NewTodoTitle, Mode=TwoWay}"
                       HorizontalOptions="FillAndExpand" />
                
                <Button Text="Add"
                        Command="{Binding AddTodoCommand}"
                        WidthRequest="80" />
            </HorizontalStackLayout>
        </VerticalStackLayout>

        <!-- Todo List -->
        <CollectionView ItemsSource="{Binding Todos}"
                        Grid.Row="1"
                        Margin="0, 20">
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:TodoItem">
                    <Frame Margin="0, 5" Padding="10" CornerRadius="5">
                        <Frame.Triggers>
                            <DataTrigger TargetType="Frame"
                                        Binding="{Binding IsCompleted}"
                                        Value="True">
                                <Setter Property="Opacity" Value="0.6" />
                            </DataTrigger>
                        </Frame.Triggers>
                        
                        <HorizontalStackLayout Spacing="10">
                            <CheckBox IsChecked="{Binding IsCompleted}"
                                      Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:TodoViewModel}}, Path=ToggleTodoCommand}"
                                      CommandParameter="{Binding .}"
                                      VerticalOptions="Center" />
                            
                            <Label Text="{Binding Title}"
                                   FontSize="16"
                                   VerticalOptions="Center"
                                   HorizontalOptions="FillAndExpand">
                                <Label.Triggers>
                                    <DataTrigger TargetType="Label"
                                               Binding="{Binding IsCompleted}"
                                               Value="True">
                                        <Setter Property="TextDecorations" Value="Strikethrough" />
                                    </DataTrigger>
                                </Label.Triggers>
                            </Label>
                            
                            <Button Text="Delete"
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:TodoViewModel}}, Path=DeleteTodoCommand}"
                                    CommandParameter="{Binding .}"
                                    BackgroundColor="Red"
                                    TextColor="White"
                                    WidthRequest="80" />
                        </HorizontalStackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Footer -->
        <Button Text="Clear Completed"
                Command="{Binding ClearCompletedCommand}"
                Grid.Row="2"
                BackgroundColor="Gray"
                TextColor="White" />

    </Grid>

</ContentPage>
```

### Step 4: Connect in Code-Behind

```csharp
// Views/TodoPage.xaml.cs
namespace MyApp.Views;

public partial class TodoPage : ContentPage
{
    public TodoPage()
    {
        InitializeComponent();
        BindingContext = new TodoViewModel();
    }
}
```

---

## 🎨 Advanced MVVM Concepts

### INotifyPropertyChanged Explained

When a property changes, the UI needs to know. `INotifyPropertyChanged` provides this notification.

**Without CommunityToolkit.Mvvm** (verbose):
```csharp
private string _name;
public string Name
{
    get => _name;
    set
    {
        if (_name != value)
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
}
```

**With CommunityToolkit.Mvvm** (clean):
```csharp
[ObservableProperty]
private string _name;
```

### ObservableCollection vs List

```csharp
// List - UI doesn't update when items added/removed
public List<TodoItem> Todos { get; set; } = new();

// ObservableCollection - UI automatically updates!
public ObservableCollection<TodoItem> Todos { get; } = new();
```

### Dependency Injection with MVVM

Register ViewModels in `MauiProgram.cs`:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts => { /* ... */ })
        
        // Register ViewModels
        .AddSingleton<TodoViewModel>()      // Single instance
        .AddTransient<LoginPageViewModel>(); // New instance each time
    
    return builder.Build();
}
```

Inject in View:

```csharp
public partial class TodoPage : ContentPage
{
    public TodoPage(TodoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Counter App with MVVM

Create a counter app using MVVM:

**ViewModel Requirements:**
- Observable property: `Count` (int, starts at 0)
- Command: `IncrementCommand` - increases count by 1
- Command: `DecrementCommand` - decreases count by 1
- Command: `ResetCommand` - sets count to 0

**View Requirements:**
- Label displaying count
- Increment button
- Decrement button (disabled when count is 0)
- Reset button (disabled when count is 0)

**Bonus**: Change label color based on count (positive = green, negative = red, zero = black)

### Exercise 2: Temperature Converter with MVVM

Create a temperature converter:

**ViewModel Requirements:**
- Observable property: `Celsius` (double)
- Observable property: `Fahrenheit` (double, calculated from Celsius)
- Command: `ConvertCommand` - performs conversion

**View Requirements:**
- Entry for Celsius input
- Label displaying Fahrenheit result
- Convert button

**Formula**: `F = C × 9/5 + 32`

**Bonus**: Make conversion automatic (no button needed) using property changed events.

### Exercise 3: Shopping List with MVVM

Create a shopping list app:

**Model:**
```csharp
public class ShoppingItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public bool IsPurchased { get; set; }
}
```

**ViewModel Requirements:**
- `ObservableCollection<ShoppingItem> Items`
- Commands: Add, Remove, TogglePurchased, ClearPurchased
- Display total items count
- Display unpurchased items count

**View Requirements:**
- Entry for item name
- Entry for quantity
- Add button
- List of items with checkboxes
- Delete button for each item
- Clear purchased button
- Labels showing counts

---

## ❓ Common Beginner Questions

### Q: Why not just use code-behind?

**A**: Code-behind mixes UI with logic, making it hard to:
- Test your code (requires UI running)
- Reuse logic across pages
- Maintain as app grows
- Collaborate with designers

### Q: When should I use code-behind?

**A**: Only for:
- UI-specific logic (animations, visual effects)
- Platform-specific code
- Things that have no business logic

### Q: What's the difference between `ObservableCollection` and `List`?

**A**: 
- `List` - Standard collection, no UI notifications
- `ObservableCollection` - Notifies UI when items added/removed/changed

### Q: Why do I need `[RelayCommand]`?

**A**: It automatically generates `ICommand` implementation, saving you from writing boilerplate code.

### Q: Can I have multiple ViewModels for one View?

**A**: Technically yes, but it's not recommended. One View = One ViewModel is the standard pattern.

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **Model** | Data structures and business logic |
| **View** | XAML UI definition |
| **ViewModel** | Bridge between Model and View |
| **Data Binding** | Automatic connection between UI and ViewModel |
| **ObservableProperty** | Property that notifies UI of changes |
| **RelayCommand** | Command that handles user actions |
| **ObservableCollection** | Collection that notifies UI of changes |

---

## ✅ Checklist

Before moving to Lesson 4, make sure you can:

- [ ] Explain the three components of MVVM
- [ ] Create a ViewModel with ObservableProperty
- [ ] Create a Command with RelayCommand
- [ ] Bind View properties to ViewModel properties
- [ ] Use ObservableCollection for lists
- [ ] Handle button clicks with commands
- [ ] Build a complete MVVM page

---

## 🚀 Next Steps

Excellent! You now understand MVVM, the professional way to build MAUI apps. In the next lesson, we'll dive deeper into **Data Binding** and learn advanced techniques for connecting UI to data.

**Next Lesson**: [Lesson 4: Data Binding](04_Data_Binding.md)

---

## 📖 Additional Reading

- [CommunityToolkit.Mvvm Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Data Binding Overview](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/)
- [MVVM Pattern Explained](https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [MVVM Samples](https://github.com/dotnet/maui-samples/tree/main/Samples)
