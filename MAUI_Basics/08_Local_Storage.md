# Lesson 8: Local Storage

## 🎯 Learning Objectives

By the end of this lesson, you will:
- Understand different local storage options in MAUI
- Master SQLite for structured data storage
- Learn Preferences API for key-value storage
- Use File System for file-based storage
- Implement secure storage with Secure Storage API
- Build a complete data persistence layer

---

## 📖 Local Storage Options

MAUI provides several ways to store data locally:

| Storage Type | Use Case | Example |
|--------------|----------|---------|
| **Preferences** | Simple key-value pairs | User settings, flags |
| **SQLite** | Structured relational data | Todos, products, users |
| **File System** | Files and documents | Images, PDFs, logs |
| **Secure Storage** | Sensitive data | Passwords, tokens |
| **Cache** | Temporary data | Images, API responses |

---

## 🔑 Preferences API

Store simple key-value pairs (strings, numbers, booleans).

### Basic Usage

```csharp
using Microsoft.Maui.Storage;

// Save values
Preferences.Set("username", "john_doe");
Preferences.Set("notifications_enabled", true);
Preferences.Set("login_count", 5);

// Get values
string username = Preferences.Get("username", "default_user");
bool notifications = Preferences.Get("notifications_enabled", false);
int count = Preferences.Get("login_count", 0);

// Check if key exists
bool hasKey = Preferences.ContainsKey("username");

// Remove key
Preferences.Remove("username");

// Clear all keys
Preferences.Clear();
```

### Creating a Settings Service

```csharp
// Services/ISettingsService.cs
public interface ISettingsService
{
    string Username { get; set; }
    bool NotificationsEnabled { get; set; }
    string Theme { get; set; }
    DateTime LastLogin { get; set; }
    void Clear();
}

// Services/SettingsService.cs
using Microsoft.Maui.Storage;

public class SettingsService : ISettingsService
{
    private const string UsernameKey = "username";
    private const string NotificationsKey = "notifications";
    private const string ThemeKey = "theme";
    private const string LastLoginKey = "last_login";
    
    public string Username
    {
        get => Preferences.Get(UsernameKey, string.Empty);
        set => Preferences.Set(UsernameKey, value);
    }
    
    public bool NotificationsEnabled
    {
        get => Preferences.Get(NotificationsKey, true);
        set => Preferences.Set(NotificationsKey, value);
    }
    
    public string Theme
    {
        get => Preferences.Get(ThemeKey, "Light");
        set => Preferences.Set(ThemeKey, value);
    }
    
    public DateTime LastLogin
    {
        get
        {
            var timestamp = Preferences.Get(LastLoginKey, 0L);
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }
        set
        {
            var timestamp = new DateTimeOffset(value).ToUnixTimeSeconds();
            Preferences.Set(LastLoginKey, timestamp);
        }
    }
    
    public void Clear()
    {
        Preferences.Remove(UsernameKey);
        Preferences.Remove(NotificationsKey);
        Preferences.Remove(ThemeKey);
        Preferences.Remove(LastLoginKey);
    }
}
```

### Register in MauiProgram.cs

```csharp
builder.Services.AddSingleton<ISettingsService, SettingsService>();
```

---

## 🗄️ SQLite Database

Store structured relational data with SQL queries.

### Step 1: Install SQLite Package

```bash
dotnet add package sqlite-net-pcl
```

Or in Visual Studio:
1. Right-click project → "Manage NuGet Packages"
2. Search for "sqlite-net-pcl"
3. Install latest version

### Step 2: Create Model

```csharp
// Models/TodoItem.cs
public class TodoItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(250)]
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public int Priority { get; set; } // 1=Low, 2=Medium, 3=High
}
```

### Step 3: Create Database Service

```csharp
// Services/IDatabaseService.cs
public interface IDatabaseService
{
    Task InitializeAsync();
    Task<List<TodoItem>> GetTodosAsync();
    Task<TodoItem> GetTodoAsync(int id);
    Task<int> AddTodoAsync(TodoItem todo);
    Task<int> UpdateTodoAsync(TodoItem todo);
    Task<int> DeleteTodoAsync(int id);
}

// Services/DatabaseService.cs
using SQLite;
using MyApp.Models;

public class DatabaseService : IDatabaseService
{
    private const string DbName = "todos.db";
    private readonly string _dbPath;
    private SQLiteAsyncConnection _connection;
    
    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
    }
    
    private async Task InitAsync()
    {
        if (_connection != null)
            return;
        
        _connection = new SQLiteAsyncConnection(_dbPath);
        await _connection.CreateTableAsync<TodoItem>();
    }
    
    public async Task InitializeAsync()
    {
        await InitAsync();
    }
    
    public async Task<List<TodoItem>> GetTodosAsync()
    {
        await InitAsync();
        return await _connection.Table<TodoItem>().ToListAsync();
    }
    
    public async Task<TodoItem> GetTodoAsync(int id)
    {
        await InitAsync();
        return await _connection.Table<TodoItem>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<int> AddTodoAsync(TodoItem todo)
    {
        await InitAsync();
        return await _connection.InsertAsync(todo);
    }
    
    public async Task<int> UpdateTodoAsync(TodoItem todo)
    {
        await InitAsync();
        return await _connection.UpdateAsync(todo);
    }
    
    public async Task<int> DeleteTodoAsync(int id)
    {
        await InitAsync();
        return await _connection.DeleteAsync<TodoItem>(id);
    }
}
```

### Step 4: Advanced Queries

```csharp
// Get completed todos
public async Task<List<TodoItem>> GetCompletedTodosAsync()
{
    await InitAsync();
    return await _connection.Table<TodoItem>()
        .Where(x => x.IsCompleted)
        .ToListAsync();
}

// Get todos by priority
public async Task<List<TodoItem>> GetTodosByPriorityAsync(int priority)
{
    await InitAsync();
    return await _connection.Table<TodoItem>()
        .Where(x => x.Priority == priority)
        .OrderByDescending(x => x.CreatedDate)
        .ToListAsync();
}

// Search todos
public async Task<List<TodoItem>> SearchTodosAsync(string searchTerm)
{
    await InitAsync();
    return await _connection.Table<TodoItem>()
        .Where(x => x.Title.Contains(searchTerm) || x.Description.Contains(searchTerm))
        .ToListAsync();
}

// Get overdue todos
public async Task<List<TodoItem>> GetOverdueTodosAsync()
{
    await InitAsync();
    var now = DateTime.Now;
    return await _connection.Table<TodoItem>()
        .Where(x => x.DueDate != null && x.DueDate < now && !x.IsCompleted)
        .ToListAsync();
}

// Count todos
public async Task<int> GetTodoCountAsync()
{
    await InitAsync();
    return await _connection.Table<TodoItem>().CountAsync();
}

// Clear all todos
public async Task<int> ClearAllTodosAsync()
{
    await InitAsync();
    return await _connection.DeleteAllAsync<TodoItem>();
}
```

### Step 5: Register in MauiProgram.cs

```csharp
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
```

### Step 6: Use in ViewModel

```csharp
public class TodoListViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    
    public TodoListViewModel(IDatabaseService database)
    {
        _database = database;
        LoadTodosCommand = new AsyncRelayCommand(LoadTodosAsync);
    }
    
    [ObservableProperty]
    private ObservableCollection<TodoItem> _todos = new();
    
    public IAsyncRelayCommand LoadTodosCommand { get; }
    
    private async Task LoadTodosAsync()
    {
        var todos = await _database.GetTodosAsync();
        Todos.Clear();
        foreach (var todo in todos)
        {
            Todos.Add(todo);
        }
    }
}
```

---

## 📁 File System Storage

Store files (images, documents, logs) on device.

### File System Paths

```csharp
using System.IO;

// App data directory (persistent)
string appDataPath = FileSystem.AppDataDirectory;

// Cache directory (may be cleared)
string cachePath = FileSystem.CacheDirectory;

// Create custom directories
string customPath = Path.Combine(FileSystem.AppDataDirectory, "MyFiles");
Directory.CreateDirectory(customPath);
```

### File Service Example

```csharp
// Services/IFileService.cs
public interface IFileService
{
    Task<string> SaveTextAsync(string filename, string content);
    Task<string> LoadTextAsync(string filename);
    Task<bool> FileExistsAsync(string filename);
    Task DeleteFileAsync(string filename);
    Task<List<string>> GetFilesAsync();
}

// Services/FileService.cs
using System.IO;

public class FileService : IFileService
{
    private readonly string _filesPath;
    
    public FileService()
    {
        _filesPath = Path.Combine(FileSystem.AppDataDirectory, "Files");
        Directory.CreateDirectory(_filesPath);
    }
    
    public async Task<string> SaveTextAsync(string filename, string content)
    {
        var filePath = Path.Combine(_filesPath, filename);
        await File.WriteAllTextAsync(filePath, content);
        return filePath;
    }
    
    public async Task<string> LoadTextAsync(string filename)
    {
        var filePath = Path.Combine(_filesPath, filename);
        return await File.ReadAllTextAsync(filePath);
    }
    
    public async Task<bool> FileExistsAsync(string filename)
    {
        var filePath = Path.Combine(_filesPath, filename);
        return await Task.FromResult(File.Exists(filePath));
    }
    
    public async Task DeleteFileAsync(string filename)
    {
        var filePath = Path.Combine(_filesPath, filename);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        await Task.CompletedTask;
    }
    
    public async Task<List<string>> GetFilesAsync()
    {
        if (!Directory.Exists(_filesPath))
            return new List<string>();
        
        return await Task.FromResult(Directory.GetFiles(_filesPath).ToList());
    }
}
```

### Save/Load Images

```csharp
public class ImageService
{
    private readonly string _imagesPath;
    
    public ImageService()
    {
        _imagesPath = Path.Combine(FileSystem.AppDataDirectory, "Images");
        Directory.CreateDirectory(_imagesPath);
    }
    
    public async Task<string> SaveImageAsync(string filename, byte[] imageBytes)
    {
        var filePath = Path.Combine(_imagesPath, filename);
        await File.WriteAllBytesAsync(filePath, imageBytes);
        return filePath;
    }
    
    public async Task<byte[]> LoadImageAsync(string filename)
    {
        var filePath = Path.Combine(_imagesPath, filename);
        return await File.ReadAllBytesAsync(filePath);
    }
    
    public ImageSource GetImageSource(string filename)
    {
        var filePath = Path.Combine(_imagesPath, filename);
        return ImageSource.FromFile(filePath);
    }
}
```

---

## 🔐 Secure Storage

Store sensitive data securely (passwords, tokens).

### Basic Usage

```csharp
using Microsoft.Maui.Storage;

// Save sensitive data
await SecureStorage.SetAsync("api_token", "your_token_here");
await SecureStorage.SetAsync("password", "encrypted_password");

// Retrieve sensitive data
string token = await SecureStorage.GetAsync("api_token");
string password = await SecureStorage.GetAsync("password");

// Remove
SecureStorage.Remove("api_token");

// Clear all
SecureStorage.RemoveAll();
```

### Secure Storage Service

```csharp
// Services/ISecureStorageService.cs
public interface ISecureStorageService
{
    Task SaveAsync(string key, string value);
    Task<string> GetAsync(string key);
    Task RemoveAsync(string key);
    Task ClearAsync();
}

// Services/SecureStorageService.cs
using Microsoft.Maui.Storage;

public class SecureStorageService : ISecureStorageService
{
    public async Task SaveAsync(string key, string value)
    {
        await SecureStorage.SetAsync(key, value);
    }
    
    public async Task<string> GetAsync(string key)
    {
        return await SecureStorage.GetAsync(key);
    }
    
    public async Task RemoveAsync(string key)
    {
        SecureStorage.Remove(key);
        await Task.CompletedTask;
    }
    
    public async Task ClearAsync()
    {
        SecureStorage.RemoveAll();
        await Task.CompletedTask;
    }
}
```

### Use Case: Remember Me

```csharp
public class LoginViewModel
{
    private readonly ISecureStorageService _secureStorage;
    private readonly ISettingsService _settings;
    
    public LoginViewModel(ISecureStorageService secureStorage, ISettingsService settings)
    {
        _secureStorage = secureStorage;
        _settings = settings;
    }
    
    [ObservableProperty]
    private string _username;
    
    [ObservableProperty]
    private string _password;
    
    [ObservableProperty]
    private bool _rememberMe;
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        // Perform login...
        
        if (_rememberMe)
        {
            await _secureStorage.SaveAsync("saved_username", _username);
            await _secureStorage.SaveAsync("saved_password", _password);
            _settings.RememberMe = true;
        }
        else
        {
            await _secureStorage.RemoveAsync("saved_username");
            await _secureStorage.RemoveAsync("saved_password");
            _settings.RememberMe = false;
        }
    }
    
    [RelayCommand]
    private async Task LoadSavedCredentialsAsync()
    {
        if (_settings.RememberMe)
        {
            Username = await _secureStorage.GetAsync("saved_username");
            Password = await _secureStorage.GetAsync("saved_password");
            RememberMe = true;
        }
    }
}
```

---

## 🎯 Complete Example: Note-Taking App

Let's build a complete note-taking app with local storage.

### Models

```csharp
public class Note
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(200)]
    public string Title { get; set; }
    
    public string Content { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime ModifiedDate { get; set; }
    
    public string Category { get; set; }
    
    public bool IsPinned { get; set; }
}
```

### Database Service

```csharp
public class NoteDatabaseService
{
    private const string DbName = "notes.db";
    private readonly string _dbPath;
    private SQLiteAsyncConnection _connection;
    
    public NoteDatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
    }
    
    private async Task InitAsync()
    {
        if (_connection != null)
            return;
        
        _connection = new SQLiteAsyncConnection(_dbPath);
        await _connection.CreateTableAsync<Note>();
    }
    
    public async Task<List<Note>> GetNotesAsync()
    {
        await InitAsync();
        return await _connection.Table<Note>()
            .OrderByDescending(n => n.ModifiedDate)
            .ToListAsync();
    }
    
    public async Task<List<Note>> GetPinnedNotesAsync()
    {
        await InitAsync();
        return await _connection.Table<Note>()
            .Where(n => n.IsPinned)
            .OrderByDescending(n => n.ModifiedDate)
            .ToListAsync();
    }
    
    public async Task<List<Note>> GetNotesByCategoryAsync(string category)
    {
        await InitAsync();
        return await _connection.Table<Note>()
            .Where(n => n.Category == category)
            .OrderByDescending(n => n.ModifiedDate)
            .ToListAsync();
    }
    
    public async Task<Note> GetNoteAsync(int id)
    {
        await InitAsync();
        return await _connection.Table<Note>()
            .Where(n => n.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<int> SaveNoteAsync(Note note)
    {
        await InitAsync();
        
        if (note.Id == 0)
        {
            note.CreatedDate = DateTime.Now;
            note.ModifiedDate = DateTime.Now;
            return await _connection.InsertAsync(note);
        }
        else
        {
            note.ModifiedDate = DateTime.Now;
            return await _connection.UpdateAsync(note);
        }
    }
    
    public async Task<int> DeleteNoteAsync(int id)
    {
        await InitAsync();
        return await _connection.DeleteAsync<Note>(id);
    }
    
    public async Task<List<string>> GetCategoriesAsync()
    {
        await InitAsync();
        var notes = await _connection.Table<Note>().ToListAsync();
        return notes.Select(n => n.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
}
```

### ViewModel

```csharp
public class NoteListViewModel : ObservableObject
{
    private readonly NoteDatabaseService _database;
    private readonly ISettingsService _settings;
    
    public NoteListViewModel(NoteDatabaseService database, ISettingsService settings)
    {
        _database = database;
        _settings = settings;
        LoadNotesCommand = new AsyncRelayCommand(LoadNotesAsync);
        AddNoteCommand = new AsyncRelayCommand(AddNoteAsync);
    }
    
    [ObservableProperty]
    private ObservableCollection<Note> _notes = new();
    
    [ObservableProperty]
    private string _selectedCategory = "All";
    
    public IAsyncRelayCommand LoadNotesCommand { get; }
    public IAsyncRelayCommand AddNoteCommand { get; }
    
    private async Task LoadNotesAsync()
    {
        List<Note> notes;
        
        if (SelectedCategory == "All")
        {
            notes = await _database.GetNotesAsync();
        }
        else if (SelectedCategory == "Pinned")
        {
            notes = await _database.GetPinnedNotesAsync();
        }
        else
        {
            notes = await _database.GetNotesByCategoryAsync(SelectedCategory);
        }
        
        Notes.Clear();
        foreach (var note in notes)
        {
            Notes.Add(note);
        }
    }
    
    private async Task AddNoteAsync()
    {
        var note = new Note
        {
            Title = "New Note",
            Content = "",
            Category = "Personal",
            IsPinned = false
        };
        
        await _database.SaveNoteAsync(note);
        await LoadNotesAsync();
        
        await Shell.Current.GoToAsync($"noteedit?noteId={note.Id}");
    }
}
```

---

## 🧪 Practice Exercises

### Exercise 1: Task Manager with SQLite

Create a task manager app:

**Model:**
```csharp
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public string Category { get; set; }
}
```

**Requirements:**
- Create database service with CRUD operations
- Implement filtering by category, priority, status
- Add search functionality
- Save user's filter preferences
- Show task count per category

### Exercise 2: Photo Gallery

Create a photo gallery app:

**Requirements:**
- Pick photos from device
- Save photos to app storage
- Display photos in CollectionView
- Add captions to photos
- Delete photos
- Use Preferences for settings (sort order, grid size)

### Exercise 3: Expense Tracker

Create an expense tracker:

**Model:**
```csharp
public class Expense
{
    public int Id { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string PaymentMethod { get; set; }
}
```

**Requirements:**
- SQLite database for expenses
- Add/edit/delete expenses
- Filter by date range and category
- Calculate totals by category
- Export to CSV (save to file system)
- Secure storage for budget limit

---

## ❓ Common Beginner Questions

### Q: When should I use SQLite vs Preferences?

**A**: 
- **Preferences**: Simple key-value, small data (settings, flags)
- **SQLite**: Structured data, relationships, queries (todos, products)

### Q: How do I handle database migrations?

**A**: 
1. Version your database
2. Check version on init
3. Run migration scripts if needed
4. Update version after migration

### Q: Is Secure Storage really secure?

**A**: Yes, it uses platform security:
- Android: Encrypted keystore
- iOS: Keychain
- Windows: DPAPI
- macOS: Keychain

### Q: Can I share data between apps?

**A**: Generally no, each app has isolated storage. Some exceptions exist (file providers, sharing extensions).

### Q: How do I backup user data?

**A**: 
1. Export to file (JSON, CSV)
2. Save to cloud storage
3. Use platform backup (iCloud, Google Backup)

---

## 📚 Key Concepts Summary

| Concept | Description |
|---------|-------------|
| **Preferences** | Simple key-value storage |
| **SQLite** | Relational database for structured data |
| **File System** | Store files and documents |
| **Secure Storage** | Encrypted storage for sensitive data |
| **FileSystem.AppDataDirectory** | Persistent app data path |
| **SQLiteAsyncConnection** | Async SQLite operations |

---

## ✅ Checklist

Before moving to Lesson 9, make sure you can:

- [ ] Use Preferences for key-value storage
- [ ] Create and use SQLite databases
- [ ] Perform CRUD operations with SQLite
- [ ] Save and load files from file system
- [ ] Use Secure Storage for sensitive data
- [ ] Build a complete data persistence layer
- [ ] Handle database migrations

---

## 🚀 Next Steps

Excellent! You now understand local storage in MAUI. In the next lesson, we'll learn about **API Integration** and how to connect your app to web services.

**Next Lesson**: [Lesson 9: API Integration](09_API_Integration.md)

---

## 📖 Additional Reading

- [Preferences API](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/preferences)
- [SQLite.NET](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-database)
- [File System](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system)
- [Secure Storage](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage)
