# SQLite Basics in .NET MAUI

## 📚 Lesson Overview

**What You'll Learn:**
- What is SQLite and why use it in mobile apps
- Setting up SQLite in .NET MAUI
- Creating database tables
- Performing CRUD operations (Create, Read, Update, Delete)
- Best practices for database management

**Difficulty:** Beginner  
**Time:** 45 minutes  
**Prerequisites:** C# Basics, MVVM Pattern (Lesson 03)

---

## 🎯 What is SQLite?

SQLite is a **lightweight, serverless** database that stores data in a single file on the device. It's perfect for mobile apps because:

✅ **No server required** - Runs directly on the device  
✅ **Zero configuration** - Works out of the box  
✅ **Fast & efficient** - Optimized for mobile devices  
✅ **Cross-platform** - Works on Android, iOS, Windows, Mac  

**Real-world example:** A todo app that saves your tasks locally so they persist even when you close the app.

---

## 🔧 Setting Up SQLite

### Step 1: Install NuGet Package

Right-click your project → Manage NuGet Packages → Search and install:

```
sqlite-net-pcl
```

**Important:** Make sure to install the version by `praeclarum` (the original author).

### Step 2: Create Your First Model

Create a simple class to represent data:

```csharp
public class TodoItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
```

**Key Attributes:**
- `[PrimaryKey]` - Unique identifier for each record
- `[AutoIncrement]` - Automatically assigns IDs (1, 2, 3...)
- `[MaxLength(100)]` - Limits text length

---

## 📁 Creating a Database Service

Create a service class to manage database operations:

```csharp
using SQLite;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class DatabaseService
{
    private const string DbName = "todos.db3";
    private readonly string _dbPath;
    
    public DatabaseService()
    {
        // Store database in app's local folder
        _dbPath = Path.Combine(
            FileSystem.AppDataDirectory, 
            DbName
        );
    }
    
    // Initialize database and create tables
    public async Task InitializeAsync()
    {
        using var connection = new SQLiteAsyncConnection(_dbPath);
        
        // Create TodoItem table if it doesn't exist
        await connection.CreateTableAsync<TodoItem>();
    }
    
    // CREATE: Add new item
    public async Task<int> AddTodoAsync(TodoItem todo)
    {
        using var connection = new SQLiteAsyncConnection(_dbPath);
        return await connection.InsertAsync(todo);
    }
    
    // READ: Get all items
    public async Task<List<TodoItem>> GetTodosAsync()
    {
        using var connection = new SQLiteAsyncConnection(_dbPath);
        return await connection.Table<TodoItem>()
                               .ToListAsync();
    }
    
    // READ: Get single item by ID
    public async Task<TodoItem?> GetTodoAsync(int id)
    {
        using var connection = new SQLiteAsyncConnection(_dbPath);
        return await connection.Table<TodoItem>()
                               .Where(t => t.Id == id)
                               .FirstOrDefaultAsync();
    }
    
    // UPDATE: Modify existing item
    public async Task<int> UpdateTodoAsync(TodoItem todo)
    {
        using var connection = new SQLiteAsyncConnection(_dbPath);
        return await connection.UpdateAsync(todo);
    }
    
    // DELETE: Remove item
    public async Task<int> DeleteTodoAsync(TodoItem todo)
    {
        using var connection = new SQLiteAsyncConnection(_dbPath);
        return await connection.DeleteAsync(todo);
    }
}
```

---

## 🎨 Using SQLite in ViewModel

Here's how to use the database service in your ViewModel:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class TodoViewModel : ObservableObject
{
    private readonly DatabaseService _database;
    
    [ObservableProperty]
    private ObservableCollection<TodoItem> _todos = new();
    
    [ObservableProperty]
    private string _newTodoTitle = string.Empty;
    
    public TodoViewModel(DatabaseService database)
    {
        _database = database;
        
        // Load todos when ViewModel is created
        Task.Run(async () => await LoadTodosAsync());
    }
    
    [RelayCommand]
    private async Task LoadTodosAsync()
    {
        var todos = await _database.GetTodosAsync();
        
        // Update UI on main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Todos.Clear();
            foreach (var todo in todos)
            {
                Todos.Add(todo);
            }
        });
    }
    
    [RelayCommand]
    private async Task AddTodoAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTodoTitle))
            return;
        
        var newTodo = new TodoItem
        {
            Title = NewTodoTitle,
            IsCompleted = false
        };
        
        await _database.AddTodoAsync(newTodo);
        NewTodoTitle = string.Empty;
        
        await LoadTodosAsync();
    }
    
    [RelayCommand]
    private async Task ToggleTodoAsync(TodoItem todo)
    {
        todo.IsCompleted = !todo.IsCompleted;
        await _database.UpdateTodoAsync(todo);
    }
    
    [RelayCommand]
    private async Task DeleteTodoAsync(TodoItem todo)
    {
        await _database.DeleteTodoAsync(todo);
        Todos.Remove(todo);
    }
}
```

---

## 🔍 Advanced Queries

### Filtering Data

```csharp
// Get only completed todos
public async Task<List<TodoItem>> GetCompletedTodosAsync()
{
    using var connection = new SQLiteAsyncConnection(_dbPath);
    return await connection.Table<TodoItem>()
                           .Where(t => t.IsCompleted == true)
                           .ToListAsync();
}

// Get todos created in last 7 days
public async Task<List<TodoItem>> GetRecentTodosAsync()
{
    using var connection = new SQLiteAsyncConnection(_dbPath);
    var weekAgo = DateTime.Now.AddDays(-7);
    
    return await connection.Table<TodoItem>()
                           .Where(t => t.CreatedDate >= weekAgo)
                           .ToListAsync();
}
```

### Sorting Data

```csharp
// Get todos sorted by date (newest first)
public async Task<List<TodoItem>> GetTodosSortedAsync()
{
    using var connection = new SQLiteAsyncConnection(_dbPath);
    return await connection.Table<TodoItem>()
                           .OrderByDescending(t => t.CreatedDate)
                           .ToListAsync();
}
```

---

## ⚠️ Common Mistakes to Avoid

### ❌ BAD: Blocking the UI Thread

```csharp
// DON'T DO THIS - Freezes the app!
public void LoadTodos()
{
    var todos = _database.GetTodosAsync().Result; // ❌
    Todos = new ObservableCollection<TodoItem>(todos);
}
```

### ✅ GOOD: Using async/await

```csharp
// DO THIS - Keeps app responsive!
public async Task LoadTodosAsync()
{
    var todos = await _database.GetTodosAsync(); // ✅
    MainThread.BeginInvokeOnMainThread(() =>
    {
        Todos.Clear();
        foreach (var todo in todos)
            Todos.Add(todo);
    });
}
```

---

## 📊 Complete Example: Todo App

### XAML View (TodoPage.xaml)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.Views.TodoPage"
             Title="My Todos">
    
    <Grid RowDefinitions="Auto,*" Padding="10">
        <!-- Input Section -->
        <HorizontalStackLayout Grid.Row="0" Spacing="10">
            <Entry Placeholder="Add new todo..."
                   Text="{Binding NewTodoTitle}"
                   HorizontalOptions="FillAndExpand"
                   HeightRequest="40"/>
            <Button Text="Add"
                    Command="{Binding AddTodoCommand}"
                    WidthRequest="80"/>
        </HorizontalStackLayout>
        
        <!-- Todo List -->
        <CollectionView Grid.Row="1"
                        ItemsSource="{Binding Todos}"
                        Margin="0,10">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Padding="10" Margin="0,5">
                        <Grid ColumnDefinitions="*,Auto,Auto">
                            <Label Grid.Column="0"
                                   Text="{Binding Title}"
                                   VerticalOptions="Center"/>
                            
                            <CheckBox Grid.Column="1"
                                      IsChecked="{Binding IsCompleted}"
                                      Command="{Binding Source={RelativeSource 
                                          AncestorType={x:Type ContentPage}}, 
                                          Path=BindingContext.ToggleTodoCommand}"
                                      CommandParameter="{Binding}"/>
                            
                            <Button Grid.Column="2"
                                    Text="Delete"
                                    Command="{Binding Source={RelativeSource 
                                        AncestorType={x:Type ContentPage}}, 
                                        Path=BindingContext.DeleteTodoCommand}"
                                    CommandParameter="{Binding}"
                                    WidthRequest="80"/>
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

### Register Service in MauiProgram.cs

```csharp
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
        
        // Register database service
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<TodoViewModel>();
        builder.Services.AddSingleton<TodoPage>();
        
        return builder.Build();
    }
}
```

---

## 🎓 Key Takeaways

1. **SQLite** stores data locally on the device in a single file
2. **Always use async/await** for database operations to keep UI responsive
3. **Use attributes** like `[PrimaryKey]` and `[AutoIncrement]` to define table structure
4. **MainThread.BeginInvokeOnMainThread()** when updating UI from background tasks
5. **Dispose connections** using `using` statements to prevent memory leaks

---

## 🚀 Next Steps

- **Lesson 12:** REST APIs - Learn to fetch data from the internet
- **Lesson 13:** Serialization - Convert objects to JSON and back

---

## 💡 Practice Exercise

**Create a Note-Taking App:**

1. Create a `Note` model with Title, Content, and CreatedDate
2. Implement CRUD operations using SQLite
3. Build a UI to add, edit, and delete notes
4. Add search functionality to filter notes by title

**Hint:** Use the same pattern as the Todo app above!

---

**Need Help?** Check the official [sqlite-net-pcl documentation](https://github.com/praeclarum/sqlite-net)
