# MVVM Basics 06: Building Your First MVVM App

## 🎯 What You'll Learn

By the end of this lesson, you will:
- Build a complete MVVM application from scratch
- See how Model, View, and ViewModel work together
- Understand the complete MVVM flow
- Learn best practices for organizing MVVM apps

---

## 📱 Project: Todo List App

We'll build a simple Todo List app that demonstrates all MVVM concepts:

### Features:
- ✅ Add new todos
- ✅ Mark todos as complete
- ✅ Delete todos
- ✅ Show todo count
- ✅ Clear completed todos

---

## 🏗️ Step 1: Project Structure

Organize your project with proper folders:

```
MyTodoApp/
├── Models/
│   └── TodoItem.cs
├── ViewModels/
│   └── TodoViewModel.cs
├── Views/
│   └── TodoPage.xaml
│   └── TodoPage.xaml.cs
├── Services/
│   └── TodoService.cs (optional, for data storage)
└── App.xaml
    └── App.xaml.cs
```

---

## 📦 Step 2: Install Required Package

Install **CommunityToolkit.Mvvm** NuGet package:

### Via Visual Studio:
1. Right-click your project → "Manage NuGet Packages"
2. Search for "CommunityToolkit.Mvvm"
3. Click "Install"

### Via Package Manager Console:
```powershell
Install-Package CommunityToolkit.Mvvm
```

---

## 📝 Step 3: Create the Model

**File**: `Models/TodoItem.cs`

```csharp
namespace MyTodoApp.Models;

/// <summary>
/// Represents a single todo item
/// </summary>
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
```

**What this does**:
- Holds todo data (id, title, completion status)
- No UI code, no business logic
- Just pure data structure

---

## 🧠 Step 4: Create the ViewModel

**File**: `ViewModels/TodoViewModel.cs`

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyTodoApp.Models;

namespace MyTodoApp.ViewModels;

/// <summary>
/// ViewModel for the Todo Page
/// </summary>
public partial class TodoViewModel : ObservableObject
{
    // Observable collection for todos
    public ObservableCollection<TodoItem> Todos { get; } = new();

    // New todo title input
    [ObservableProperty]
    private string _newTodoTitle;

    // App title
    [ObservableProperty]
    private string _appTitle = "My Todo App";

    // Statistics
    [ObservableProperty]
    private int _totalTodos;

    [ObservableProperty]
    private int _completedTodos;

    [ObservableProperty]
    private int _pendingTodos;

    // Add a new todo
    [RelayCommand]
    private void AddTodo()
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(NewTodoTitle))
        {
            Application.Current?.MainPage?.DisplayAlert("Error", 
                "Please enter a todo title", "OK");
            return;
        }

        // Create new todo
        var todo = new TodoItem
        {
            Id = Todos.Count + 1,
            Title = NewTodoTitle.Trim(),
            IsCompleted = false
        };

        // Add to collection
        Todos.Add(todo);

        // Clear input
        NewTodoTitle = string.Empty;

        // Update statistics
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

    // Clear all completed todos
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
        PendingTodos = Todos.Count(t => !t.IsCompleted);
    }

    // Load sample data (for testing)
    [RelayCommand]
    private void LoadSampleData()
    {
        Todos.Clear();
        
        Todos.Add(new TodoItem { Id = 1, Title = "Learn MVVM", IsCompleted = true });
        Todos.Add(new TodoItem { Id = 2, Title = "Build MAUI app", IsCompleted = false });
        Todos.Add(new TodoItem { Id = 3, Title = "Master data binding", IsCompleted = false });
        
        UpdateStats();
    }
}
```

**What this does**:
- Manages todo list state
- Handles user actions (add, delete, toggle)
- Updates statistics
- No UI code, just logic

---

## 🎨 Step 5: Create the View

**File**: `Views/TodoPage.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:MyTodoApp.ViewModels"
             xmlns:models="clr-namespace:MyTodoApp.Models"
             x:Class="MyTodoApp.Views.TodoPage"
             x:DataType="viewmodels:TodoViewModel"
             Title="{Binding AppTitle}">

    <!-- Set BindingContext to ViewModel -->
    <ContentPage.BindingContext>
        <viewmodels:TodoViewModel />
    </ContentPage.BindingContext>

    <!-- Main Layout -->
    <Grid RowDefinitions="Auto, Auto, *, Auto" Padding="20">

        <!-- Header with Statistics -->
        <Frame Grid.Row="0"
               BackgroundColor="{StaticResource Primary}"
               Padding="15"
               CornerRadius="10">
            <VerticalStackLayout Spacing="5">
                <Label Text="Statistics"
                       FontSize="14"
                       TextColor="White"
                       FontAttributes="Bold"/>
                <Grid ColumnDefinitions="*,*,*">
                    <StackLayout Grid.Column="0">
                        <Label Text="{Binding TotalTodos}"
                               FontSize="24"
                               TextColor="White"
                               FontAttributes="Bold"
                               HorizontalOptions="Center"/>
                        <Label Text="Total"
                               FontSize="12"
                               TextColor="White"
                               HorizontalOptions="Center"/>
                    </StackLayout>
                    <StackLayout Grid.Column="1">
                        <Label Text="{Binding CompletedTodos}"
                               FontSize="24"
                               TextColor="White"
                               FontAttributes="Bold"
                               HorizontalOptions="Center"/>
                        <Label Text="Done"
                               FontSize="12"
                               TextColor="White"
                               HorizontalOptions="Center"/>
                    </StackLayout>
                    <StackLayout Grid.Column="2">
                        <Label Text="{Binding PendingTodos}"
                               FontSize="24"
                               TextColor="White"
                               FontAttributes="Bold"
                               HorizontalOptions="Center"/>
                        <Label Text="Pending"
                               FontSize="12"
                               TextColor="White"
                               HorizontalOptions="Center"/>
                    </StackLayout>
                </Grid>
            </VerticalStackLayout>
        </Frame>

        <!-- Add New Todo Section -->
        <Frame Grid.Row="1"
               Padding="15"
               Margin="0,10,0,10"
               CornerRadius="10">
            <Grid ColumnDefinitions="*,Auto">
                <Entry Grid.Column="0"
                       Placeholder="What needs to be done?"
                       Text="{Binding NewTodoTitle, Mode=TwoWay}"
                       Margin="0,0,10,0"/>
                <Button Grid.Column="1"
                        Text="Add"
                        Command="{Binding AddTodoCommand}"
                        WidthRequest="80"
                        CornerRadius="8"/>
            </Grid>
        </Frame>

        <!-- Todo List -->
        <CollectionView Grid.Row="2"
                        ItemsSource="{Binding Todos}"
                        EmptyView="No todos yet. Add one above!">
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:TodoItem">
                    <Frame Margin="0,5"
                           Padding="15"
                           CornerRadius="8"
                           HasShadow="True">
                        <Frame.Triggers>
                            <DataTrigger TargetType="Frame"
                                        Binding="{Binding IsCompleted}"
                                        Value="True">
                                <Setter Property="Opacity" Value="0.6"/>
                                <Setter Property="BackgroundColor" Value="#E8F5E9"/>
                            </DataTrigger>
                        </Frame.Triggers>

                        <Grid ColumnDefinitions="Auto,*,Auto">
                            <!-- Checkbox -->
                            <CheckBox Grid.Column="0"
                                      IsChecked="{Binding IsCompleted}"
                                      Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:TodoViewModel}}, Path=ToggleTodoCommand}"
                                      CommandParameter="{Binding .}"
                                      VerticalOptions="Center"
                                      Margin="0,0,10,0"/>

                            <!-- Title -->
                            <Label Grid.Column="1"
                                   Text="{Binding Title}"
                                   FontSize="16"
                                   VerticalOptions="Center"
                                   LineBreakMode="WordWrap">
                                <Label.Triggers>
                                    <DataTrigger TargetType="Label"
                                               Binding="{Binding IsCompleted}"
                                               Value="True">
                                        <Setter Property="TextDecorations" Value="Strikethrough"/>
                                    </DataTrigger>
                                </Label.Triggers>
                            </Label>

                            <!-- Delete Button -->
                            <Button Grid.Column="2"
                                    Text="Delete"
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:TodoViewModel}}, Path=DeleteTodoCommand}"
                                    CommandParameter="{Binding .}"
                                    BackgroundColor="#EF5350"
                                    TextColor="White"
                                    WidthRequest="80"
                                    CornerRadius="8"
                                    HeightRequest="35"/>
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Footer Actions -->
        <Button Grid.Row="3"
                Text="Clear Completed"
                Command="{Binding ClearCompletedCommand}"
                BackgroundColor="Gray"
                TextColor="White"
                CornerRadius="8"
                Margin="0,10,0,0"/>

    </Grid>

</ContentPage>
```

**What this does**:
- Displays the UI
- Binds to ViewModel properties
- Handles user interactions through Commands
- No business logic, just presentation

---

## 🔧 Step 6: Code-Behind (Minimal)

**File**: `Views/TodoPage.xaml.cs`

```csharp
namespace MyTodoApp.Views;

public partial class TodoPage : ContentPage
{
    public TodoPage()
    {
        InitializeComponent();
    }
}
```

**What this does**:
- Just initializes the component
- No business logic here!
- The ViewModel handles everything

---

## 🚀 Step 7: Set as Main Page

**File**: `AppShell.xaml.cs`

```csharp
namespace MyTodoApp;

public partial class AppShell : AppShell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Set TodoPage as the main page
        Routing.RegisterRoute("todo", typeof(Views.TodoPage));
    }
}
```

**File**: `App.xaml.cs`

```csharp
namespace MyTodoApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
```

---

## 🎯 How It All Works Together

### The Complete Flow:

```
┌─────────────────────────────────────────────────────────┐
│                    USER ACTION                          │
│              User types "Learn MVVM"                     │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                      VIEW (XAML)                        │
│  <Entry Text="{Binding NewTodoTitle, Mode=TwoWay}" />   │
│  → Updates NewTodoTitle in ViewModel                    │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  VIEWMODEL (C#)                         │
│  [ObservableProperty]                                    │
│  private string _newTodoTitle;                          │
│  → Property changes, notifies View                      │
│                                                         │
│  [RelayCommand]                                         │
│  void AddTodo() {                                       │
│    Todos.Add(new TodoItem {...});                       │
│  }                                                      │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                     MODEL (C#)                          │
│  public class TodoItem {                                │
│    public int Id { get; set; }                          │
│    public string Title { get; set; }                    │
│    public bool IsCompleted { get; set; }                │
│  }                                                      │
└─────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              BACK TO VIEW (Auto Update)                 │
│  <CollectionView ItemsSource="{Binding Todos}" />       │
│  → Automatically shows new todo                         │
└─────────────────────────────────────────────────────────┘
```

---

## ✅ Testing Your App

### Test All Features:

1. **Add Todo**:
   - Type in the Entry field
   - Click "Add" button
   - Todo appears in the list

2. **Toggle Todo**:
   - Click the checkbox
   - Todo gets strikethrough
   - Statistics update

3. **Delete Todo**:
   - Click "Delete" button
   - Todo disappears
   - Statistics update

4. **Clear Completed**:
   - Complete some todos
   - Click "Clear Completed"
   - Completed todos are removed

---

## 🎨 Customizing the UI

### Change Colors:

```xml
<!-- In App.xaml or TodoPage.xaml Resources -->
<Color x:Key="Primary">#512BD4</Color>
<Color x:Key="Secondary">#FFD040</Color>
<Color x:Key="Success">#4CAF50</Color>
<Color x:Key="Danger">#F44336</Color>
```

### Add Icons:

```xml
<Button Text="Add"
        Command="{Binding AddTodoCommand}"
        ImageSource="add_icon.png"/>
```

### Add Animations:

```xml
<Frame>
    <Frame.Behaviors>
        <behaviors:AnimationBehavior />
    </Frame.Behaviors>
</Frame>
```

---

## 🎯 Best Practices Used

### ✅ What We Did Right:

1. **Proper Separation**:
   - Model: Just data
   - ViewModel: Just logic
   - View: Just UI

2. **Data Binding**:
   - Used `TwoWay` for input fields
   - Used `OneWay` (default) for display
   - Used Commands for actions

3. **ObservableCollection**:
   - Used for collections that change
   - View updates automatically

4. **Minimal Code-Behind**:
   - Only InitializeComponent()
   - No business logic

5. **Clean Code**:
   - Clear naming
   - Comments for clarity
   - Organized structure

---

## 🚀 Next Steps to Enhance

### Add These Features:

1. **Data Persistence**:
   ```csharp
   // Save to local database
   await _databaseService.SaveTodoAsync(todo);
   ```

2. **Search/Filter**:
   ```xml
   <SearchBar Text="{Binding SearchText}"
              SearchCommand="{Binding SearchCommand}"/>
   ```

3. **Categories**:
   ```csharp
   public class TodoItem
   {
       public string Category { get; set; }
   }
   ```

4. **Due Dates**:
   ```csharp
   public class TodoItem
   {
       public DateTime? DueDate { get; set; }
   }
   ```

5. **Priority Levels**:
   ```csharp
   public enum Priority { Low, Medium, High }
   ```

---

## ✅ Quick Check

**Question**: What happens when you type in the Entry field?

<details>
<summary>Answer</summary>

The Entry is bound to `NewTodoTitle` with `Mode=TwoWay`, so as you type, the `NewTodoTitle` property in the ViewModel updates automatically.

</details>

---

**Question**: How does the list update when you add a todo?

<details>
<summary>Answer</summary>

When you click "Add", the `AddTodoCommand` adds a new `TodoItem` to the `Todos` collection. Since `Todos` is an `ObservableCollection`, it automatically notifies the View, and the `CollectionView` updates to show the new todo.

</details>

---

**Question**: Why is there no code in the code-behind file?

<details>
<summary>Answer</summary>

In MVVM, all logic goes in the ViewModel. The code-behind should only contain `InitializeComponent()`. This keeps the UI separate from the logic.

</details>

---

## 🎓 Key Takeaways

1. **MVVM = Separation**: Model, View, and ViewModel each have their own job
2. **Data Binding = Magic**: Automatically keeps UI and logic synchronized
3. **Commands = Actions**: Handle user interactions without code-behind
4. **ObservableCollection = Live Lists**: Collections that notify when changed
5. **Practice = Mastery**: Build more apps to get comfortable

---

## 📚 Next Steps

Congratulations on building your first MVVM app! Now learn more advanced patterns:

- **Next**: [MVVM Basics 07: Common MVVM Patterns](MVVM_Basics_07_Patterns.md)
- **Then**: [MVVM Basics 08: Dependency Injection](MVVM_Basics_08_Dependency_Injection.md)

---

## 💡 Practice Exercise

**Try This**: Add a "Priority" feature to the todo app

**Requirements**:
- Add Priority property to TodoItem (Low, Medium, High)
- Add a Picker to select priority when adding
- Show priority indicator (color) on each todo
- Sort todos by priority

<details>
<summary>Hint</summary>

1. Add `Priority` enum and property to `TodoItem`
2. Add `Priority` property to ViewModel
3. Add Picker to View bound to `Priority`
4. Use DataTrigger to color-code by priority

</details>

---

**Remember**: This is just the beginning! MVVM is a powerful pattern that scales from simple apps like this to enterprise applications. Keep practicing!
