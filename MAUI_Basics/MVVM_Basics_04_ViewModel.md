# MVVM Basics 04: Understanding the ViewModel

## 🎯 What You'll Learn

By the end of this lesson, you will:
- Understand what the ViewModel is
- Learn how to create ViewModels
- Know what belongs in a ViewModel
- Understand Observable properties
- Learn how to create Commands
- See how ViewModels connect Models to Views

---

## 📖 What is the ViewModel?

The **ViewModel** is the **bridge** between the Model and the View. It prepares data for display and handles user actions.

### Think of the ViewModel as:
- 🌉 **A bridge** connecting Model to View
- 🧠 **The brain** that makes decisions
- 🎬 **The director** coordinating everything

### What the ViewModel Does:
✅ Prepares data from Model for View
✅ Handles user actions (button clicks, etc.)
✅ Validates user input
✅ Notifies View when data changes
✅ Contains business logic for the View
❌ **NOT** UI code (no Colors, Fonts)
❌ **NOT** direct database access (use Services)
❌ **NOT** complex business rules (use Model)

---

## 🏗️ ViewModel Structure

A ViewModel is a C# class that inherits from `ObservableObject` and uses special attributes.

### Basic ViewModel Example

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    // Observable property (notifies View when changed)
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

### What This Does:
- `ObservableObject`: Base class that enables change notification
- `[ObservableProperty]`: Creates a property that notifies the View when it changes
- `[RelayCommand]`: Creates a command that can handle button clicks
- `partial class`: Required for code generation

---

## 🔄 Observable Properties

Observable properties automatically notify the View when they change, so the UI updates automatically.

### Creating Observable Properties

```csharp
public partial class CounterViewModel : ObservableObject
{
    // Private field (starts with underscore)
    [ObservableProperty]
    private int _count = 0;

    // This automatically generates:
    // public int Count
    // {
    //     get => _count;
    //     set => SetProperty(ref _count, value);
    // }
}
```

### Using Observable Properties

```xml
<!-- In your View -->
<Label Text="{Binding Count}" />
```

When `Count` changes in the ViewModel, the Label automatically updates!

### Multiple Properties

```csharp
public partial class UserViewModel : ObservableObject
{
    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private bool _isActive;

    // Computed property (read-only)
    public string FullName => $"{FirstName} {LastName}";
}
```

---

## 🔘 Commands

Commands handle user actions without code-behind. They're methods that can be bound to buttons and other controls.

### Creating Commands

```csharp
public partial class MyViewModel : ObservableObject
{
    // Simple command
    [RelayCommand]
    private void Save()
    {
        // Save logic here
    }

    // Async command
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // Async operations
        await Task.Delay(1000);
    }

    // Command with parameter
    [RelayCommand]
    private void DeleteItem(string itemId)
    {
        // Delete the item with this ID
    }
}
```

### Using Commands in Views

```xml
<!-- Simple command -->
<Button Text="Save"
        Command="{Binding SaveCommand}" />

<!-- Async command -->
<Button Text="Load Data"
        Command="{Binding LoadDataAsyncCommand}" />

<!-- Command with parameter -->
<Button Text="Delete"
        Command="{Binding DeleteItemCommand}"
        CommandParameter="{Binding Id}" />
```

---

## 🎯 Complete ViewModel Example

### Todo List ViewModel

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class TodoViewModel : ObservableObject
{
    // Observable collection (notifies View when items added/removed)
    public ObservableCollection<TodoItem> Todos { get; } = new();

    [ObservableProperty]
    private string _newTodoTitle;

    [ObservableProperty]
    private string _appTitle = "My Todo App";

    [ObservableProperty]
    private int _totalTodos;

    [ObservableProperty]
    private int _completedTodos;

    // Add a new todo
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
        UpdateStats();
    }

    // Delete a todo
    [RelayCommand]
    private void DeleteTodo(TodoItem todo)
    {
        Todos.Remove(todo);
        UpdateStats();
    }

    // Toggle todo completion
    [RelayCommand]
    private void ToggleTodo(TodoItem todo)
    {
        todo.IsCompleted = !todo.IsCompleted;
        UpdateStats();
    }

    // Clear completed todos
    [RelayCommand]
    private void ClearCompleted()
    {
        var completed = Todos.Where(t => t.IsCompleted).ToList();
        foreach (var todo in completed)
        {
            Todos.Remove(todo);
        }
        UpdateStats();
    }

    // Update statistics
    private void UpdateStats()
    {
        TotalTodos = Todos.Count;
        CompletedTodos = Todos.Count(t => t.IsCompleted);
    }
}
```

---

## 🎨 CanExecute: Enable/Disable Commands

Commands can be enabled or disabled based on conditions.

### Example: Form Validation

```csharp
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _password;

    // Command with CanExecute
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        // Login logic
    }

    // This determines if the command can execute
    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Password) &&
               Password.Length >= 6;
    }

    // Call these when properties change
    partial void OnUsernameChanged(string value)
    {
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        LoginCommand.NotifyCanExecuteChanged();
    }
}
```

```xml
<!-- Button automatically enables/disables -->
<Button Text="Login"
        Command="{Binding LoginCommand}" />
```

---

## 🔄 OnPropertyChanged Callbacks

You can run code when a property changes.

```csharp
public partial class UserProfileViewModel : ObservableObject
{
    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    // This runs automatically when FirstName changes
    partial void OnFirstNameChanged(string value)
    {
        Console.WriteLine($"First name changed to: {value}");
        // You could trigger validation, save to database, etc.
    }

    // This runs automatically when LastName changes
    partial void OnLastNameChanged(string value)
    {
        Console.WriteLine($"Last name changed to: {value}");
    }
}
```

---

## 🎯 Best Practices for ViewModels

### ✅ DO:
- Keep ViewModels **focused on one View**
- Use **ObservableProperty** for all properties the View needs
- Use **RelayCommand** for all user actions
- Put **business logic** in ViewModels
- **Validate** user input in ViewModels
- Keep ViewModels **testable** (no UI dependencies)

### ❌ DON'T:
- Put UI code in ViewModels (no Colors, Fonts)
- Access database directly (use Services)
- Make ViewModels too large (split if needed)
- Put complex business rules in ViewModels (use Model)
- Mix concerns (keep it focused)

---

## 📊 ViewModel vs. Model vs. View

| Feature | Model | ViewModel | View |
|---------|-------|-----------|------|
| **Purpose** | Holds data | Connects Model to View | Shows UI |
| **UI Code** | Never | Sometimes | Always |
| **Commands** | Never | Yes | Never |
| **Notifications** | No | Yes | No |
| **Example** | `User`, `Product` | `UserViewModel`, `ProductViewModel` | `UserPage.xaml` |

---

## 🛠️ Creating Your First ViewModel

### Step 1: Create a ViewModels Folder

```
MyApp/
├── Models/
├── ViewModels/
│   └── MainPageViewModel.cs
├── Views/
```

### Step 2: Install CommunityToolkit.Mvvm

```bash
# In Package Manager Console
Install-Package CommunityToolkit.Mvvm
```

Or via Visual Studio:
1. Right-click project → "Manage NuGet Packages"
2. Search for "CommunityToolkit.Mvvm"
3. Click "Install"

### Step 3: Create the ViewModel

```csharp
// ViewModels/MainPageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = "World";

    [RelayCommand]
    private void Greet()
    {
        Name = "MAUI Developer";
    }
}
```

### Step 4: Connect to View

```xml
<!-- Views/MainPage.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.MainPage"
             x:DataType="viewmodels:MainPageViewModel">
    
    <ContentPage.BindingContext>
        <viewmodels:MainPageViewModel />
    </ContentPage.BindingContext>
    
    <StackLayout Padding="20">
        <Label Text="{Binding Name}"
               FontSize="24"
               HorizontalOptions="Center" />
        
        <Button Text="Greet Me"
                Command="{Binding GreetCommand}" />
    </StackLayout>
    
</ContentPage>
```

---

## ✅ Quick Check

**Question**: What is the main purpose of a ViewModel?

<details>
<summary>Answer</summary>

To **connect the Model to the View** - preparing data for display and handling user actions.

</details>

---

**Question**: What does `[ObservableProperty]` do?

<details>
<summary>Answer</summary>

It automatically generates a property that **notifies the View when it changes**, so the UI updates automatically.

</details>

---

**Question**: What does `[RelayCommand]` do?

<details>
<summary>Answer</summary>

It automatically generates a **Command** that can handle user actions like button clicks.

</details>

---

**Question**: Can a ViewModel have UI code like Colors?

<details>
<summary>Answer</summary>

**No!** ViewModels should not contain UI code. That belongs in the View.

</details>

---

## 🎓 Key Takeaways

1. **ViewModels = Bridge**: They connect Models to Views
2. **ObservableProperty**: Notifies View when data changes
3. **RelayCommand**: Handles user actions
4. **No UI Code**: Keep ViewModels focused on logic
5. **Testable**: ViewModels should be easy to test without UI

---

## 📚 Next Steps

Now that you understand all three parts of MVVM, let's see how they work together:

- **Next**: [MVVM Basics 05: Data Binding Deep Dive](MVVM_Basics_05_Data_Binding.md)
- **Then**: [MVVM Basics 06: Building Your First MVVM App](MVVM_Basics_06_First_App.md)

---

## 💡 Practice Exercise

**Try This**: Create a ViewModel for a simple calculator

**Requirements**:
- Two number inputs (Num1, Num2)
- Result property
- Add command
- Subtract command
- Multiply command
- Divide command

<details>
<summary>See Solution</summary>

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public partial class CalculatorViewModel : ObservableObject
{
    [ObservableProperty]
    private double _num1;

    [ObservableProperty]
    private double _num2;

    [ObservableProperty]
    private string _result = "0";

    [RelayCommand]
    private void Add()
    {
        Result = (Num1 + Num2).ToString();
    }

    [RelayCommand]
    private void Subtract()
    {
        Result = (Num1 - Num2).ToString();
    }

    [RelayCommand]
    private void Multiply()
    {
        Result = (Num1 * Num2).ToString();
    }

    [RelayCommand]
    private void Divide()
    {
        if (Num2 != 0)
            Result = (Num1 / Num2).ToString();
        else
            Result = "Cannot divide by zero";
    }
}
```

</details>

---

**Remember**: The ViewModel is the heart of MVVM. Master it, and you'll master MVVM!
